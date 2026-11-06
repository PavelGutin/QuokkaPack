import { Trip } from '../data/model';
import { daysUntil, todayIso, tripPhase } from '../data/ops';

const fmt = new Intl.DateTimeFormat(undefined, { month: 'short', day: 'numeric' });
const fmtYear = new Intl.DateTimeFormat(undefined, { month: 'short', day: 'numeric', year: 'numeric' });

function d(iso: string) {
  const [y, m, day] = iso.split('-').map(Number);
  return new Date(y, m - 1, day);
}

export function dateRange(trip: Trip): string {
  const { startDate: s, endDate: e } = trip;
  if (!s && !e) return 'No dates';
  const thisYear = String(new Date().getFullYear());
  const f = (iso: string) => (iso.startsWith(thisYear) ? fmt : fmtYear).format(d(iso));
  if (!e || s === e) return f(s || e);
  if (!s) return `Until ${f(e)}`;
  return `${f(s)} – ${f(e)}`;
}

export function whenLabel(trip: Trip): string {
  const today = todayIso();
  switch (tripPhase(trip, today)) {
    case 'undated':
      return '';
    case 'ongoing':
      return 'Happening now';
    case 'past':
      return 'Done';
    case 'upcoming': {
      const n = daysUntil(trip.startDate || trip.endDate, today);
      return n === 1 ? 'Tomorrow' : n < 14 ? `In ${n} days` : n < 60 ? `In ${Math.round(n / 7)} weeks` : `In ${Math.round(n / 30)} months`;
    }
  }
}

const fmtMonth = new Intl.DateTimeFormat(undefined, { month: 'short' });

/** Compact "when" for list rows: "Tomorrow", "13 d", "2 mo", "Now", or the month of a past trip. */
export function shortWhen(trip: Trip): string {
  const today = todayIso();
  switch (tripPhase(trip, today)) {
    case 'undated':
      return '';
    case 'ongoing':
      return 'Now';
    case 'past':
      return fmtMonth.format(d(trip.endDate || trip.startDate));
    case 'upcoming': {
      const n = daysUntil(trip.startDate || trip.endDate, today);
      return n === 1 ? 'Tomorrow' : n < 60 ? `${n} d` : `${Math.round(n / 30)} mo`;
    }
  }
}

/** Two-letter label for a category on the jump rail. */
export function railLabel(name: string): string {
  return name.trim().slice(0, 2);
}
