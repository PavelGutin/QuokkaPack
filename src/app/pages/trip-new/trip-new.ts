import { Component, computed, inject, input, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import * as ops from '../../data/ops';
import { Store } from '../../data/store';
import { dateRange } from '../../ui/format';

@Component({
  selector: 'qp-trip-new',
  imports: [FormsModule, RouterLink],
  templateUrl: './trip-new.html',
})
export class TripNewPage implements OnInit {
  store = inject(Store);
  private router = inject(Router);

  /** ?from=<tripId> pre-selects a trip to copy. */
  from = input<string>();

  name = '';
  startDate = '';
  endDate = '';
  source = signal<'categories' | 'copy'>('categories');
  copyFrom = signal('');
  selected = signal<Set<string>>(new Set());

  categories = computed(() => {
    const items = this.store.items().filter((i) => !i.archived);
    return this.store
      .categories()
      .filter((c) => !c.archived)
      .map((c) => ({ ...c, count: items.filter((i) => i.categoryId === c.id).length }))
      .filter((c) => c.count)
      .sort(ops.byName);
  });

  otherTrips = computed(() => {
    const { active, past } = ops.sortTrips(this.store.trips());
    return [...active, ...past].filter((t) => t.items.length);
  });

  selectedCount = computed(() => this.categories().filter((c) => this.selected().has(c.id)).reduce((n, c) => n + c.count, 0));

  dateRange = dateRange;

  ngOnInit() {
    if (this.from() && this.store.trips().some((t) => t.id === this.from())) {
      this.copyFrom.set(this.from()!);
      this.source.set('copy');
    }
  }

  toggle(id: string) {
    const s = new Set(this.selected());
    s.has(id) ? s.delete(id) : s.add(id);
    this.selected.set(s);
  }

  selectAll(on: boolean) {
    this.selected.set(on ? new Set(this.categories().map((c) => c.id)) : new Set());
  }

  onStartChange() {
    if (this.startDate && (!this.endDate || this.endDate < this.startDate)) this.endDate = this.startDate;
  }

  submit() {
    const copy = this.source() === 'copy' ? this.copyFrom() : '';
    const categoryIds = this.source() === 'categories' ? [...this.selected()] : [];
    let id = '';
    const ok = this.store.update((d) => {
      const r = ops.createTrip(
        d,
        { name: this.name, startDate: this.startDate, endDate: this.endDate },
        { categoryIds, copyFromTripId: copy || undefined },
      );
      id = r.id;
      return r.doc;
    });
    if (!ok) return;
    const empty = !categoryIds.length && !copy;
    this.router.navigate(['/trips', id], { queryParams: empty ? { mode: 'edit' } : {}, replaceUrl: true });
  }
}
