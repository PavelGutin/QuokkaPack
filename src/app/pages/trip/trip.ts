import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Category, Item, TripItem } from '../../data/model';
import * as ops from '../../data/ops';
import { Store } from '../../data/store';
import { BarHeight } from '../../ui/bar-height';
import { ClickOutside } from '../../ui/click-outside';
import { dateRange, railLabel, whenLabel } from '../../ui/format';
import { ToastService } from '../../ui/toast.service';

interface EditRow {
  item: Item;
  tripItem?: TripItem;
}
interface EditGroup {
  category: Category;
  rows: EditRow[];
  included: number;
}

const matches = (name: string, q: string) => !q || name.toLocaleLowerCase().includes(q);
const same = (a: string, b: string) => a.trim().toLocaleLowerCase() === b.trim().toLocaleLowerCase();

@Component({
  selector: 'qp-trip',
  imports: [FormsModule, RouterLink, ClickOutside, BarHeight],
  templateUrl: './trip.html',
})
export class TripPage {
  store = inject(Store);
  private router = inject(Router);
  private toast = inject(ToastService);

  id = input.required<string>();
  /** Query params: ?mode=edit, ?details=1 */
  modeParam = input<string>(undefined, { alias: 'mode' });
  details = input<string>();

  mode = signal<'pack' | 'edit'>('pack');
  query = signal('');
  hidePacked = signal(false);
  menuOpen = signal(false);
  editingDetails = signal(false);
  /** Fully packed categories the person opened again. */
  expanded = signal<Set<string>>(new Set());
  draft = { name: '', startDate: '', endDate: '', notes: '' };
  newCategory = '';
  addCategoryId = signal('');

  trip = computed(() => this.store.trips().find((t) => t.id === this.id()));
  stats = computed(() => ops.tripStats(this.trip()!));
  q = computed(() => this.query().trim().toLocaleLowerCase());
  categories = computed(() => this.store.categories().filter((c) => !c.archived).sort(ops.byName));

  packGroups = computed(() => {
    const trip = this.trip();
    if (!trip) return [];
    const q = this.q();
    const hide = this.hidePacked();
    const open = this.expanded();
    return ops
      .tripGroups(this.store.doc(), trip)
      .map((g) => {
        const done = g.packed === g.lines.length;
        const collapsed = done && !q && !open.has(g.category.id);
        return {
          ...g,
          done,
          collapsed,
          visible: collapsed ? [] : g.lines.filter((l) => matches(l.item.name, q) && !(hide && l.tripItem.packed)),
        };
      })
      .filter((g) => g.visible.length || (!q && !(hide && g.done)));
  });

  editGroups = computed<EditGroup[]>(() => {
    const trip = this.trip();
    if (!trip) return [];
    const doc = this.store.doc();
    const onTrip = new Map(trip.items.map((ti) => [ti.itemId, ti]));
    const q = this.q();
    const groups: EditGroup[] = [];
    for (const category of [...doc.categories].sort(ops.byName)) {
      const all = doc.items
        .filter((i) => i.categoryId === category.id && (!i.archived || onTrip.has(i.id)))
        .map((item) => ({ item, tripItem: onTrip.get(item.id) }));
      const included = all.filter((r) => r.tripItem).length;
      if (category.archived && !included) continue;
      const rows = all.filter((r) => matches(r.item.name, q) || matches(category.name, q)).sort((a, b) => ops.byName(a.item, b.item));
      if (q && !rows.length) continue;
      groups.push({ category, rows, included });
    }
    return groups;
  });

  /** Rail entries for whichever list is showing. */
  rail = computed(() => {
    const cats = this.mode() === 'pack' ? this.packGroups().map((g) => g.category) : this.editGroups().map((g) => g.category);
    return cats.length > 3 ? cats.map((c) => ({ id: c.id, name: c.name, label: railLabel(c.name) })) : [];
  });

  /** The typed name, when it isn't already a catalog item, so it can be added. */
  newItemName = computed(() => {
    const raw = this.query().trim();
    if (!raw) return '';
    return this.store.items().some((i) => same(i.name, raw)) ? '' : raw;
  });

  dateRange = dateRange;
  whenLabel = whenLabel;

  constructor() {
    effect(() => {
      if (this.store.ready() && !this.trip()) this.router.navigate(['/'], { replaceUrl: true });
    });
    effect(() => {
      this.mode.set(this.modeParam() === 'edit' ? 'edit' : 'pack');
    });
    effect(() => {
      if (this.details() && this.trip()) this.openDetails();
    });
    effect(() => {
      const cats = this.categories();
      if (!cats.some((c) => c.id === this.addCategoryId())) this.addCategoryId.set(cats[0]?.id ?? '');
    });
  }

  jump(categoryId: string) {
    document.getElementById('cat-' + categoryId)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  /* ---- pack mode ---- */

  togglePacked(ti: TripItem) {
    this.store.update((d) => ops.setPacked(d, this.id(), [ti.id], !ti.packed));
    if (!ti.packed && this.stats().percent === 100) this.toast.show('Everything is packed. Have a great trip!', { kind: 'success' });
  }

  packGroup(lines: { tripItem: TripItem }[], packed: boolean) {
    this.store.update((d) => ops.setPacked(d, this.id(), lines.map((l) => l.tripItem.id), packed));
  }

  toggleExpanded(categoryId: string) {
    const s = new Set(this.expanded());
    s.has(categoryId) ? s.delete(categoryId) : s.add(categoryId);
    this.expanded.set(s);
  }

  /* ---- edit mode ---- */

  toggleIncluded(r: EditRow) {
    if (r.tripItem) this.store.update((d) => ops.removeItemsFromTrip(d, this.id(), [r.item.id]));
    else this.store.update((d) => ops.addItemsToTrip(d, this.id(), [r.item.id]));
  }

  includeAll(g: EditGroup, on: boolean) {
    const ids = g.rows.filter((r) => !r.item.archived || !on).map((r) => r.item.id);
    this.store.update((d) => (on ? ops.addItemsToTrip(d, this.id(), ids) : ops.removeItemsFromTrip(d, this.id(), ids)));
  }

  qty(ti: TripItem, delta: number) {
    this.store.update((d) => ops.setQuantity(d, this.id(), ti.id, ti.quantity + delta));
  }

  addTyped() {
    const name = this.newItemName();
    const categoryId = this.addCategoryId();
    if (!name || !categoryId) return;
    const cat = this.categories().find((c) => c.id === categoryId);
    if (this.store.update((d) => ops.quickAddToTrip(d, this.id(), categoryId, name))) {
      this.query.set('');
      this.toast.show(`Added “${name}” to ${cat?.name} and this trip`);
    }
  }

  addCategory() {
    if (!this.newCategory.trim()) return;
    let id = '';
    if (
      this.store.update((d) => {
        const r = ops.addCategory(d, this.newCategory);
        id = r.id;
        return r.doc;
      })
    ) {
      this.newCategory = '';
      this.addCategoryId.set(id);
    }
  }

  /* ---- trip actions ---- */

  openDetails() {
    const t = this.trip()!;
    this.draft = { name: t.name, startDate: t.startDate, endDate: t.endDate, notes: t.notes };
    this.editingDetails.set(true);
    this.menuOpen.set(false);
  }

  saveDetails() {
    if (this.store.update((d) => ops.updateTrip(d, this.id(), this.draft))) this.closeDetails();
  }

  closeDetails() {
    this.editingDetails.set(false);
    if (this.details()) this.router.navigate([], { queryParams: { details: null }, queryParamsHandling: 'merge', replaceUrl: true });
  }

  onStartChange() {
    if (this.draft.startDate && (!this.draft.endDate || this.draft.endDate < this.draft.startDate)) this.draft.endDate = this.draft.startDate;
  }

  duplicate() {
    this.menuOpen.set(false);
    let newId = '';
    this.store.update((d) => {
      const r = ops.duplicateTrip(d, this.id());
      newId = r.id;
      return r.doc;
    });
    if (newId) this.router.navigate(['/trips', newId], { queryParams: { details: 1 } });
  }

  resetPacking() {
    this.menuOpen.set(false);
    this.store.update((d) => ops.resetPacking(d, this.id()), 'Unchecked all items');
  }

  remove() {
    const t = this.trip()!;
    this.menuOpen.set(false);
    this.router.navigate(['/']);
    this.store.update((d) => ops.deleteTrip(d, t.id), `Deleted “${t.name}”`);
  }
}
