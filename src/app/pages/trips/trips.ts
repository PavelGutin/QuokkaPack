import { NgTemplateOutlet } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Trip } from '../../data/model';
import * as ops from '../../data/ops';
import { Store } from '../../data/store';
import { ClickOutside } from '../../ui/click-outside';
import { dateRange, whenLabel } from '../../ui/format';

@Component({
  selector: 'qp-trips',
  imports: [RouterLink, ClickOutside, NgTemplateOutlet],
  templateUrl: './trips.html',
})
export class TripsPage {
  store = inject(Store);
  private router = inject(Router);

  sorted = computed(() => ops.sortTrips(this.store.trips()));
  showOnboarding = computed(() => !this.store.doc().onboarded && this.store.items().length === 0);
  openMenu = signal<string | null>(null);
  showPast = signal(false);

  dateRange = dateRange;
  whenLabel = whenLabel;
  stats = ops.tripStats;
  phase = (t: Trip) => ops.tripPhase(t);

  useStarter() {
    this.store.update((d) => ops.addStarterCatalog(d));
  }

  skipStarter() {
    this.store.update((d) => ops.apply(d, (x) => (x.onboarded = true)));
  }

  toggleMenu(id: string, e: Event) {
    e.preventDefault();
    e.stopPropagation();
    this.openMenu.set(this.openMenu() === id ? null : id);
  }

  duplicate(t: Trip) {
    this.openMenu.set(null);
    let newId = '';
    this.store.update((d) => {
      const r = ops.duplicateTrip(d, t.id);
      newId = r.id;
      return r.doc;
    });
    if (newId) this.router.navigate(['/trips', newId], { queryParams: { details: 1 } });
  }

  remove(t: Trip) {
    this.openMenu.set(null);
    this.store.update((d) => ops.deleteTrip(d, t.id), `Deleted “${t.name}”`);
  }
}
