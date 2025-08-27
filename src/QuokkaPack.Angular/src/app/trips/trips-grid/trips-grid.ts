import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TripSummaryReadDto, TripItemReadDto, ItemReadDto, CategoryReadDto, TripEditDto, TripItemCreateDto } from '../../core/models/api-types';
import { TripCard } from '../trip-card/trip-card';  

@Component({
  standalone: true,
  selector: 'app-trips-grid',
  imports: [CommonModule, TripCard],
  templateUrl: './trips-grid.html',
  styleUrls: ['./trips-grid.scss'],
  //changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TripsGrid {
  @Input({ required: true }) trips: TripSummaryReadDto[] = [];

  @Output() view = new EventEmitter<number | string>();
  @Output() pack = new EventEmitter<number | string>();
  @Output() edit = new EventEmitter<number | string>();
  @Output() remove = new EventEmitter<number | string>();

  formatDate(raw: string | Date): string {
    const d = new Date(raw);
    return isNaN(d.getTime())
      ? String(raw)
      : new Intl.DateTimeFormat(undefined, { year: 'numeric', month: 'short', day: '2-digit' }).format(d);
  }
}
