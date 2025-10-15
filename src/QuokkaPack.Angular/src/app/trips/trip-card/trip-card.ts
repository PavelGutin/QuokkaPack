import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TripSummaryReadDto } from '../../core/models/api-types';

@Component({
  standalone: true,
  selector: 'app-trip-card',
  imports: [CommonModule],
  templateUrl: './trip-card.html',
  styleUrls: ['./trip-card.scss'],
})
export class TripCard {
  @Input({ required: true }) trip!: TripSummaryReadDto;

  @Output() view = new EventEmitter<number | string>();
  @Output() pack = new EventEmitter<number | string>();
  @Output() edit = new EventEmitter<number | string>();
  @Output() remove = new EventEmitter<number | string>();

  get packingProgress(): string {
    if ((this.trip.totalItems ?? 0) === 0) return '0%';
    const percent = Math.round(((this.trip.packedItems ?? 0) / (this.trip.totalItems ?? 1)) * 100);
    return `${percent}%`;
  }

  get itemsSummary(): string {
    return `${this.trip.packedItems ?? 0}/${this.trip.totalItems ?? 0} packed`;
  }

  // Simple date helpers (keep same display as grid)
  fmt(raw: string | Date | undefined): string {
    if (!raw) return '';
    const d = new Date(raw);
    return isNaN(d.getTime())
      ? String(raw)
      : new Intl.DateTimeFormat(undefined, { year: 'numeric', month: 'short', day: '2-digit' }).format(d);
  }

  iso(raw: string | Date | undefined): string {
    if (!raw) return '';
    const d = new Date(raw);
    return isNaN(d.getTime()) ? '' : d.toISOString().substring(0, 10);
  }

  onDelete(event: Event) {
    event.stopPropagation(); // Prevent card click from firing
    this.remove.emit(this.trip.id);
  }
}
