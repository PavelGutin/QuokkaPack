import { NgTemplateOutlet } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Trip } from '../../data/model';
import * as ops from '../../data/ops';
import { Store } from '../../data/store';
import { DriveSync } from '../../sync/drive-sync.service';
import { dateRange, shortWhen, whenLabel } from '../../ui/format';

@Component({
  selector: 'qp-trips',
  imports: [RouterLink, NgTemplateOutlet],
  templateUrl: './trips.html',
})
export class TripsPage {
  store = inject(Store);
  sync = inject(DriveSync);

  sorted = computed(() => ops.sortTrips(this.store.trips()));
  /** The soonest upcoming or ongoing trip, shown large. */
  next = computed(() => this.sorted().active[0]);
  others = computed(() => this.sorted().active.slice(1));
  showOnboarding = computed(() => !this.store.doc().onboarded && this.store.items().length === 0);
  showPast = signal(false);

  nextStats = computed(() => {
    const t = this.next();
    if (!t) return null;
    const s = ops.tripStats(t);
    const groups = ops.tripGroups(this.store.doc(), t);
    return { ...s, left: s.total - s.packed, groups: groups.length, groupsDone: groups.filter((g) => g.packed === g.lines.length).length };
  });

  dateRange = dateRange;
  whenLabel = whenLabel;
  shortWhen = shortWhen;
  stats = ops.tripStats;

  kicker(t: Trip): string {
    const w = whenLabel(t);
    if (ops.tripPhase(t) === 'ongoing') return 'Happening now';
    return w ? `Next trip · ${w.toLocaleLowerCase()}` : 'Next trip';
  }

  rowSub(t: Trip): string {
    const s = ops.tripStats(t);
    const when = dateRange(t);
    if (!s.total) return `${when} · no items yet`;
    if (!s.packed) return `${when} · ${s.total} items`;
    return `${when} · ${s.packed} of ${s.total} packed`;
  }

  useStarter() {
    this.store.update((d) => ops.addStarterCatalog(d));
  }

  skipStarter() {
    this.store.update((d) => ops.apply(d, (x) => (x.onboarded = true)));
  }
}
