import { Component, computed, inject, input, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import * as ops from '../../data/ops';
import { Store } from '../../data/store';

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
  notes = '';
  copyFrom = '';
  selected = signal<Set<string>>(new Set());

  categories = computed(() => {
    const items = this.store.items().filter((i) => !i.archived);
    return this.store
      .categories()
      .filter((c) => !c.archived)
      .map((c) => ({ ...c, items: items.filter((i) => i.categoryId === c.id).sort(ops.byName) }))
      .filter((c) => c.items.length)
      .sort(ops.byName);
  });

  otherTrips = computed(() => {
    const { active, past } = ops.sortTrips(this.store.trips());
    return [...active, ...past].filter((t) => t.items.length);
  });

  ngOnInit() {
    if (this.from() && this.store.trips().some((t) => t.id === this.from())) this.copyFrom = this.from()!;
  }

  toggle(id: string) {
    const s = new Set(this.selected());
    s.has(id) ? s.delete(id) : s.add(id);
    this.selected.set(s);
  }

  selectAll(on: boolean) {
    this.selected.set(on ? new Set(this.categories().map((c) => c.id)) : new Set());
  }

  preview(items: { name: string }[]) {
    const names = items.slice(0, 6).map((i) => i.name);
    return names.join(', ') + (items.length > 6 ? ` +${items.length - 6} more` : '');
  }

  onStartChange() {
    if (this.startDate && (!this.endDate || this.endDate < this.startDate)) this.endDate = this.startDate;
  }

  submit() {
    let id = '';
    const ok = this.store.update((d) => {
      const r = ops.createTrip(
        d,
        { name: this.name, startDate: this.startDate, endDate: this.endDate, notes: this.notes },
        { categoryIds: [...this.selected()], copyFromTripId: this.copyFrom || undefined },
      );
      id = r.id;
      return r.doc;
    });
    if (!ok) return;
    const empty = !this.selected().size && !this.copyFrom;
    this.router.navigate(['/trips', id], { queryParams: empty ? { mode: 'edit' } : {}, replaceUrl: true });
  }
}
