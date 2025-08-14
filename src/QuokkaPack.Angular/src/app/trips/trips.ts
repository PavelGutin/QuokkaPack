import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

type Trip = {
  id: number;
  destination: string;
  startDate: string;
  endDate: string;
  categories: any[];
};

@Component({
  standalone: true,
  selector: 'app-trips',
  imports: [CommonModule],
  styles: [`
    :host { display:block; padding: 1.25rem; }
    .page-header {
      display:flex; gap:.75rem; align-items:center; justify-content:space-between; flex-wrap:wrap; margin-bottom: 1rem;
    }
    .title { font-size: 1.75rem; font-weight: 600; margin: 0; }
    .actions { display:flex; gap:.5rem; }
    button, .link-btn {
      appearance: none; border: 1px solid #ccc; background: #f8f8f8; padding: .45rem .75rem;
      border-radius: .5rem; cursor: pointer; font: inherit; line-height: 1.2;
    }
    button:hover, .link-btn:hover { background:#f0f0f0; }
    .btn-primary { border-color: #2b6cb0; background:#2b6cb0; color:white; }
    .btn-primary:hover { background:#245c96; }

    .error { color:#b00020; margin:.5rem 0 0; }
    .subtle { color:#666; }
    .loading { color:#444; font-style: italic; margin:.25rem 0 .5rem; }

    .grid {
      display:grid; grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
      gap: 1rem; margin-top: .5rem;
    }
    .card {
      border:1px solid #e5e5e5; border-radius: .75rem; padding: .85rem .9rem;
      background:white; box-shadow: 0 1px 2px rgba(0,0,0,.04);
      display:flex; flex-direction:column; gap:.6rem;
    }
    .card-header { display:flex; align-items:center; justify-content:space-between; gap:.5rem; }
    .dest { font-weight:600; font-size:1.05rem; }
    .dates { font-size:.925rem; color:#444; }
    .meta { font-size:.85rem; color:#666; }
    .card-actions { display:flex; flex-wrap:wrap; gap:.5rem; margin-top:.25rem; }
    .empty {
      border:1px dashed #ccc; border-radius:.75rem; padding:1rem; text-align:center; color:#666; background:#fafafa;
    }
  `],
  template: `
    <div class="page-header">
      <h1 class="title">My Trips</h1>
      <div class="actions">
        <button class="btn-primary" (click)="addTrip()">Add Trip</button>
      </div>
    </div>

    <p *ngIf="loading" class="loading">Loading…</p>
    <p *ngIf="error" class="error">{{ error }}</p>

    <div *ngIf="!loading && trips?.length; else maybeEmpty" class="grid">
      <div class="card" *ngFor="let t of trips; trackBy: trackById">
        <div class="card-header">
          <div class="dest">{{ t.destination }}</div>
          <div class="dates">{{ formatDate(t.startDate) }} – {{ formatDate(t.endDate) }}</div>
        </div>
        <div class="meta">
          {{ (t.categories?.length || 0) }} categor{{ (t.categories?.length || 0) === 1 ? 'y' : 'ies' }}
        </div>
        <div class="card-actions">
          <button (click)="viewTrip(t.id)">View</button>
          <button (click)="packTrip(t.id)">Pack</button>
          <button (click)="editTrip(t.id)">Edit</button>
          <button (click)="deleteTrip(t.id)">Delete</button>
        </div>
      </div>
    </div>

    <ng-template #maybeEmpty>
      <ng-container *ngIf="!loading && !error">
        <div class="empty">
          <div class="subtle">No trips yet.</div>
          <div style="margin-top:.5rem;">
            <button class="btn-primary" (click)="addTrip()">Add Trip</button>
          </div>
        </div>
      </ng-container>
    </ng-template>
  `
})
export class Trips implements OnInit {
  trips: Trip[] = [];
  error = '';
  loading = false;

  private dtf = new Intl.DateTimeFormat(undefined, { year:'numeric', month:'short', day:'2-digit' });

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.fetchTrips(); // auto-load on component mount
  }

  private fetchTrips() {
    this.loading = true;
    this.error = '';
    this.http.get<Trip[]>('/api/trips').subscribe({
      next: r => this.trips = Array.isArray(r) ? r : [],
      error: e => this.error = e?.error || e?.message || 'Error',
      complete: () => this.loading = false
    });
  }

  // Actions (wire up real navigation later)
  addTrip() { alert('Add Trip (stub)'); }
  viewTrip(id: number) { alert(`View Trip ${id} (stub)`); }
  packTrip(id: number) { alert(`Pack Trip ${id} (stub)`); }
  editTrip(id: number) { alert(`Edit Trip ${id} (stub)`); }
  deleteTrip(id: number) { alert(`Delete Trip ${id} (stub)`); }

  trackById = (_: number, t: Trip) => t.id;

  formatDate(raw: string) {
    const d = new Date(raw);
    return isNaN(d.getTime()) ? raw : this.dtf.format(d);
  }
}
