import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TripsService } from '../../core/features/trips/trips.service';
import { Trip, TripItemReadDto, ItemReadDto, CategoryReadDto, TripEditDto, TripItemCreateDto } from '../../core/models/api-types';



@Component({
  standalone: true,
  selector: 'app-trip-edit',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './trip-edit.html',
  styleUrls: ['./trip-edit.scss'],
})
export class TripEdit implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private trips = inject(TripsService);

  loading = signal(true);
  saving = signal(false);
  error = signal<string>('');

  tripId = signal<number | null>(null);
  trip = signal<Trip | null>(null);
  tripItems = signal<TripItemReadDto[]>([]);
  allItems = signal<ItemReadDto[]>([]);
  allCategories = signal<CategoryReadDto[]>([]);

  // form model
  destination = signal<string>('');
  startDate = signal<string>(''); // yyyy-MM-dd for <input type="date">
  endDate = signal<string>('');

  // selects for “add item/category”
  selectedItemId = signal<number | null>(null);
  selectedCategoryId = signal<number | null>(null);

  /** filter out items already in trip to reduce duplicates in the add-item dropdown */
  availableItems = computed(() => {
    const inTripIds = new Set(this.tripItems().map(ti => ti.itemReadDto.id));
    return this.allItems().filter(i => !inTripIds.has(i.id));
  });



  get destinationText(): string {
    return this.destination();
  }
  set destinationText(v: string) {
    this.destination.set(v);
  }

  get startDateText(): string {
    return this.startDate();
  }
  set startDateText(v: string) {
    this.startDate.set(v);
  }

  get endDateText(): string {
    return this.endDate();
  }
  set endDateText(v: string) {
    this.endDate.set(v);
  }


  get selectedItemIdValue(): number | null {
    return this.selectedItemId();
  }
  set selectedItemIdValue(v: number | string | null) {
    // coerce in case something passes a string
    this.selectedItemId.set(v === null || v === '' ? null : Number(v));
  }

  get selectedCategoryIdValue(): number | null { 
    return this.selectedCategoryId();
  }
  set selectedCategoryIdValue(v: number | string | null) {
    this.selectedCategoryId.set(v === null || v === '' ? null : Number(v));
  }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isFinite(id)) {
      this.error.set('Invalid trip id.');
      this.loading.set(false);
      return;
    }
    this.tripId.set(id);
    this.load(id); 
  }

  private load(id: number) {
    this.loading.set(true);
    this.error.set('');

    // Load trip, trip items, all items, all categories in parallel-ish (simple chaining)
    this.trips.get(id).subscribe({
      next: (t) => {
        this.trip.set(t);
        this.destination.set(t.destination ?? '');
        this.startDate.set(toInputDate(t.startDate));
        this.endDate.set(toInputDate(t.endDate));
      },
      error: (e) => this.error.set(e?.error || e?.message || 'Failed to load trip'),
    });

    this.trips.getItems(id).subscribe({
      next: (items) => this.tripItems.set(items ?? []),
      error: (e) => this.error.set(e?.error || e?.message || 'Failed to load trip items'),
    });

    this.trips.listAllItems().subscribe({
      next: (items) => this.allItems.set(items ?? []),
      error: (e) => this.error.set(e?.error || e?.message || 'Failed to load items list'),
    });

    this.trips.listAllCategories().subscribe({
      next: (cats) => this.allCategories.set(cats ?? []),
      error: (e) => this.error.set(e?.error || e?.message || 'Failed to load categories list'),
      complete: () => this.loading.set(false),
    });
  }

  saveTrip() {
    const id = this.tripId();
    if (!id) return;

    const payload: TripEditDto = {
      id,
      destination: this.destination().trim(),
      startDate: this.startDate(),
      endDate: this.endDate(),
    };

    this.saving.set(true);
    this.error.set('');

    this.trips.update(payload as any).subscribe({
      next: () => { /* nothing else to do */ },
      error: (e) => this.error.set(e?.error || e?.message || 'Failed to update trip'),
      complete: () => this.saving.set(false),
    });
  }

  addItem() {
    const id = this.tripId();
    const itemId = this.selectedItemId();
    if (!id || !itemId) return;

    const body: TripItemCreateDto = { itemId, isPacked: false };
    this.trips.addTripItem(id, body).subscribe({
      next: (created) => {
        // Refresh trip items after add
        this.trips.getItems(id).subscribe({
          next: (items) => this.tripItems.set(items ?? []),
          complete: () => this.selectedItemId.set(null),
        });
      },
      error: (e) => this.error.set(e?.error || e?.message || 'Failed to add item to trip'),
    });
  }

  deleteItem(tripItemId: number) {
    const id = this.tripId();
    if (!id) return;

    this.trips.deleteTripItem(id, tripItemId).subscribe({
      next: () => {
        this.tripItems.set(this.tripItems().filter(ti => ti.id !== tripItemId));
      },
      error: (e) => this.error.set(e?.error || e?.message || 'Failed to delete item from trip'),
    });
  }

  addCategory() {
    const id = this.tripId();
    const catId = this.selectedCategoryId();
    if (!id || !catId) return;

    this.trips.addCategoryToTrip(id, catId).subscribe({
      next: () => {
        // Re-load trip to reflect categories
        this.trips.get(id).subscribe({
          next: (t) => this.trip.set(t),
          complete: () => this.selectedCategoryId.set(null),
        });
      },
      error: (e) => this.error.set(e?.error || e?.message || 'Failed to add category'),
    });
  }

  deleteCategory(categoryId: number) {
    const id = this.tripId();
    if (!id) return;

    this.trips.deleteCategoryFromTrip(id, categoryId).subscribe({
      next: () => {
        // prune locally without a full reload
        const t = this.trip();
        if (t) {
          const next = { ...t, categories: (t.categories ?? []).filter((c: any) => c.id !== categoryId) };
          this.trip.set(next);
        }
      },
      error: (e) => this.error.set(e?.error || e?.message || 'Failed to delete category'),
    });
  }

  backToTrips() {
    this.router.navigate(['/trips']);
  }
}

/** Helpers */
function toInputDate(d: Date | string): string {
  const date = typeof d === 'string' ? new Date(d) : d;
  if (!(date instanceof Date) || isNaN(date.getTime())) return '';
  // yyyy-MM-dd
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const dd = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${dd}`;
}
