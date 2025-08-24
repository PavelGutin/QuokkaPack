import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Trip } from '../../core/models/trip'

@Component({
  standalone: true,
  selector: 'app-trip-card',
  imports: [CommonModule],
  templateUrl: './trip-card.html',
  styleUrls: ['./trip-card.scss'],
  //changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TripCard {
  @Input({ required: true }) trip!: Trip;

  @Output() view = new EventEmitter<number | string>();
  @Output() pack = new EventEmitter<number | string>();
  @Output() edit = new EventEmitter<number | string>();
  @Output() remove = new EventEmitter<number | string>();

  get categoryCount(): number {
    return this.trip?.categories?.length ?? 0;
  }

  // Simple date helpers (keep same display as grid)
  fmt(raw: string | Date): string {
    const d = new Date(raw);
    return isNaN(d.getTime())
      ? String(raw)
      : new Intl.DateTimeFormat(undefined, { year: 'numeric', month: 'short', day: '2-digit' }).format(d);
  }

  iso(raw: string | Date): string {
    const d = new Date(raw);
    return isNaN(d.getTime()) ? '' : d.toISOString().substring(0, 10);
  }
}
