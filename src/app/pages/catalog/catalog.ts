import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Category, Item } from '../../data/model';
import * as ops from '../../data/ops';
import { Store } from '../../data/store';
import { BarHeight } from '../../ui/bar-height';
import { ClickOutside } from '../../ui/click-outside';
import { SyncChip } from '../../ui/sync-chip';
import { ToastService } from '../../ui/toast.service';
import { railLabel } from '../../ui/format';
import { InlineEdit } from '../../ui/inline-edit';

@Component({
  selector: 'qp-catalog',
  imports: [FormsModule, InlineEdit, ClickOutside, RouterLink, BarHeight, SyncChip],
  templateUrl: './catalog.html',
})
export class CatalogPage {
  store = inject(Store);
  private toast = inject(ToastService);
  query = signal('');
  newCategoryOpen = signal(false);
  addCategoryId = signal('');
  showArchived = signal(false);
  newCategory = '';
  openMenu = signal<string | null>(null);

  archivedCount = computed(
    () => this.store.categories().filter((c) => c.archived).length + this.store.items().filter((i) => i.archived).length,
  );

  /** How many trips use each item. */
  usage = computed(() => {
    const m = new Map<string, number>();
    for (const t of this.store.trips()) for (const ti of t.items) m.set(ti.itemId, (m.get(ti.itemId) ?? 0) + 1);
    return m;
  });

  groups = computed(() => {
    const q = this.query().trim().toLocaleLowerCase();
    const arch = this.showArchived();
    const items = this.store.items();
    return [...this.store.categories()]
      .filter((c) => arch || !c.archived)
      .sort(ops.byName)
      .map((category) => {
        const catHit = !q || category.name.toLocaleLowerCase().includes(q);
        const list = items
          .filter((i) => i.categoryId === category.id && (arch || !i.archived))
          .filter((i) => catHit || i.name.toLocaleLowerCase().includes(q))
          .sort(ops.byName);
        return { category, items: list, visible: catHit || list.length > 0 };
      })
      .filter((g) => g.visible);
  });

  activeCategories = computed(() => this.store.categories().filter((c) => !c.archived).sort(ops.byName));

  rail = computed(() => {
    const g = this.groups();
    return g.length > 3 ? g.map((x) => ({ id: x.category.id, name: x.category.name, label: railLabel(x.category.name) })) : [];
  });

  /** The typed text, when no item has that name yet, so it can be added. */
  newItemName = computed(() => {
    const raw = this.query().trim();
    if (!raw) return '';
    const l = raw.toLocaleLowerCase();
    return this.store.items().some((i) => i.name.trim().toLocaleLowerCase() === l) ? '' : raw;
  });

  constructor() {
    effect(() => {
      const cats = this.activeCategories();
      if (!cats.some((c) => c.id === this.addCategoryId())) this.addCategoryId.set(cats[0]?.id ?? '');
    });
  }

  jump(categoryId: string) {
    document.getElementById('cat-' + categoryId)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  addTyped() {
    const name = this.newItemName();
    const cat = this.activeCategories().find((c) => c.id === this.addCategoryId());
    if (!name || !cat) return;
    if (this.store.update((d) => ops.addItem(d, cat.id, name).doc)) {
      this.query.set('');
      this.toast.show(`Added “${name}” to ${cat.name}`);
    }
  }

  addCategory() {
    if (!this.newCategory.trim()) return;
    if (this.store.update((d) => ops.addCategory(d, this.newCategory).doc)) {
      this.newCategory = '';
      this.newCategoryOpen.set(false);
    }
  }

  renameCategory(c: Category, name: string) {
    this.store.update((d) => ops.renameCategory(d, c.id, name));
  }

  archiveCategory(c: Category, archived: boolean) {
    this.openMenu.set(null);
    this.store.update((d) => ops.setCategoryArchived(d, c.id, archived), archived ? `Archived “${c.name}”` : undefined);
  }

  deleteCategory(c: Category, count: number) {
    this.openMenu.set(null);
    this.store.update((d) => ops.deleteCategory(d, c.id), `Deleted “${c.name}”${count ? ` and ${count} item${count === 1 ? '' : 's'}` : ''}`);
  }

  renameItem(i: Item, name: string) {
    this.store.update((d) => ops.updateItem(d, i.id, { name }));
  }

  moveItem(i: Item, categoryId: string) {
    this.openMenu.set(null);
    const cat = this.store.categories().find((c) => c.id === categoryId);
    this.store.update((d) => ops.updateItem(d, i.id, { categoryId }), `Moved “${i.name}” to ${cat?.name}`);
  }

  archiveItem(i: Item, archived: boolean) {
    this.openMenu.set(null);
    this.store.update((d) => ops.setItemArchived(d, i.id, archived), archived ? `Archived “${i.name}”` : undefined);
  }

  deleteItem(i: Item) {
    this.openMenu.set(null);
    this.store.update((d) => ops.deleteItem(d, i.id), `Deleted “${i.name}”`);
  }
}
