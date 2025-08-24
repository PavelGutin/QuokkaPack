import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TripsGrid } from '../trips-grid/trips-grid';
import { Trip } from '../../core/models/trip'
import { TripsService } from '../../core/features/trips/trips.service';
import { finalize } from 'rxjs/operators';
import { Router } from '@angular/router';

@Component({
  standalone: true,
  selector: 'app-trips-page',
  imports: [CommonModule, TripsGrid],
  templateUrl: './trips-page.html',
  styleUrls: ['./trips-page.scss'],
  //changeDetection: ChangeDetectionStrategy.OnPush,  //TODO: Bring this back and fix it
})
export class TripsPage implements OnInit {
  trips: Trip[] = [];
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
    console.log('Fetching trips…');

    this.tripsSvc
      .list()
      .pipe(finalize(() => {
        this.loading = false;
        console.log('Fetch complete. loading =', this.loading, 'error =', this.error);
      }))
      .subscribe({
        next: trips => {
          console.log('Service response (domain Trips):', trips);
          this.trips = Array.isArray(trips) ? trips : [];
          console.log('Parsed trips array:', this.trips);
        },
        error: e => {
          console.error('Service error:', e);
          this.error = e?.error || e?.message || 'Error';
        }
      });
}

  // Actions (stubbed; wire up routing later)
  addTrip() { alert('Add Trip (stub)'); }
  viewTrip(id: number | string) { alert(`View Trip ${id} (stub)`); }
  //packTrip(id: number | string) { alert(`Pack Trip ${id} (stub)`); }
  packTrip(id: number | string) { this.router.navigate(['/trips', id, 'pack']); }
  editTrip(id: number | string) { alert(`Edit Trip ${id} (stub)`); }
  deleteTrip(id: number | string) { alert(`Delete Trip ${id} (stub)`); }
}