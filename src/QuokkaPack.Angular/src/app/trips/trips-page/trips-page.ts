import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TripsGrid } from '../trips-grid/trips-grid';
import { TripSummaryReadDto, TripItemReadDto, ItemReadDto, CategoryReadDto, TripEditDto, TripItemCreateDto } from '../../core/models/api-types';
import { TripsService } from '../../core/features/trips/trips.service';
import { finalize } from 'rxjs/operators';
import { Router } from '@angular/router';

@Component({
  standalone: true,
  selector: 'app-trips-page',
  imports: [CommonModule, TripsGrid],
  templateUrl: './trips-page.html',
  styleUrls: ['./trips-page.scss']
})
export class TripsPage implements OnInit {
  trips: TripSummaryReadDto[] = [];
  loading = false;
  error = '';
  private router = inject(Router);

  constructor() {}

  private tripsSvc = inject(TripsService);
  
  ngOnInit(): void {
    this.fetchTrips();
  }

private fetchTrips() {
    this.loading = true;
    this.error = '';

    this.tripsSvc
      .list()
      .pipe(finalize(() => {
        this.loading = false;
      }))
      .subscribe({
        next: trips => {
          this.trips = Array.isArray(trips) ? trips : [];
        },
        error: e => {
          this.error = e?.error || e?.message || 'Error';
        }
      });
}

  // Actions (stubbed; wire up routing later)
  addTrip() { this.router.navigate(['/trips/create']); }
  viewTrip(id: number | string) { alert(`View Trip ${id} (stub)`); }
  //packTrip(id: number | string) { alert(`Pack Trip ${id} (stub)`); }
  packTrip(id: number | string) { this.router.navigate(['/trips', id, 'pack']); }
  editTrip(id: number | string) { this.router.navigate(['/trips', id, 'edit']); }

  deleteTrip(id: number | string) {
    const trip = this.trips.find(t => t.id === +id);
    const tripName = trip?.destination || `Trip ${id}`;

    if (!confirm(`Delete trip "${tripName}"? This cannot be undone.`)) {
      return;
    }

    this.tripsSvc.remove(+id).subscribe({
      next: () => {
        this.trips = this.trips.filter(t => t.id !== +id);
      },
      error: e => {
        this.error = e?.error?.message || e?.error || e?.message || 'Failed to delete trip';
      }
    });
  }
}