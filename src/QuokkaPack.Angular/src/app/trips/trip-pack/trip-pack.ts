import {
  Component,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
  inject,
  signal,
  computed,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { TripsService } from '../../core/features/trips/trips.service';
import {
  TripDetailsReadDto,
  TripEditDto,
  TripCatalogItemReadDto,
  TripItemCreateDto,
  TripItemEditDto
} from '../../core/models/api-types';
type Group = {
  categoryId?: number | null;
  category: string;
  items: TripCatalogItemReadDto[];
  hasTripItems: boolean; // true if any item in this group has a non-null tripItemId
};

@Component({
  selector: 'app-trip-pack',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './trip-pack.html',
})


export class TripPack implements OnInit, OnChanges {
  private tripsService = inject(TripsService);
  private route = inject(ActivatedRoute);

  @Input({ required: false }) tripId?: number;

  mode = signal<'pack' | 'edit'>('pack');
  trip = signal<TripDetailsReadDto | null>(null);
  loading = signal<boolean>(false);
  error = signal<string>('');
  private loadedForId: number | null = null;

  // --- Edit buffer (uses string dates for form inputs) ---
  edit: { id: number; startDate: string; endDate: string; destination: string } | null = null;

  // --- Pack mode local state: tripItemId -> packed? ---
  packedMap = signal<Record<number, boolean>>({});



private baseGroups = computed<Group[]>(() => {
  const t = this.trip();
  const by = new Map<
    string,
    { categoryId?: number | null; category: string; items: TripCatalogItemReadDto[]; hasTripItems: boolean }
  >();

  for (const it of t?.items ?? []) {
    const category = it.categoryName ?? '(Uncategorized)';
    const key = `${it.categoryId ?? 'null'}|${category}`;

    if (!by.has(key)) {
      by.set(key, { categoryId: it.categoryId ?? null, category, items: [], hasTripItems: false });
    }

    const g = by.get(key)!;
    g.items.push(it);
    if (it.tripItemId != null) g.hasTripItems = true;
  }

  return Array.from(by.values())
    .sort((a, b) => (a.category ?? '').localeCompare(b.category ?? ''))
    .map(g => ({
      categoryId: g.categoryId,
      category: g.category,
      hasTripItems: g.hasTripItems,
      items: g.items.slice().sort((a, b) => a.name.localeCompare(b.name)),
    }));
});

// Groups with at least one item in trip
groupsInTrip = computed<Group[]>(() => this.baseGroups()
  .filter(g => g.hasTripItems));

// Groups where all items are not in trip
availableGroups = computed<Group[]>(() => this.baseGroups()
  .filter(g => !g.hasTripItems));

packGroups = computed<Group[]>(() => this.baseGroups()
  .map(g => ({...g,items: g.items.filter(it => it.tripItemId != null),}))
  .filter(g => g.items.length > 0)
);


  ngOnInit(): void {
    const idFromRoute = Number(this.route.snapshot.paramMap.get('id'));
    const id = this.tripId ?? (Number.isFinite(idFromRoute) ? idFromRoute : undefined);
    if (id) this.loadTrip(id);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ('tripId' in changes && this.tripId && this.tripId !== this.loadedForId) {
      this.loadTrip(this.tripId);
    }
  }

  setMode(m: 'pack' | 'edit') {
    this.mode.set(m); // no API
    if (m === 'edit') this.seedEditBuffer();
    if (m === 'pack') this.seedPackedMap();
  }

  private seedEditBuffer() {
    const t = this.trip();
    if (!t) return;
    this.edit = {
      id: t.id,
      startDate: this.formatDateToInput(t.startDate),
      endDate: this.formatDateToInput(t.endDate),
      destination: t.destination ?? '',
    };
  }

  private seedPackedMap() {
    const map: Record<number, boolean> = {};
    for (const it of this.trip()?.items ?? []) {
      if (it.tripItemId != null) map[it.tripItemId] = !!it.isPacked;
    }
    this.packedMap.set(map);
  }

  private loadTrip(id: number) {
    this.loading.set(true);
    this.error.set('');
    this.tripsService
      .get(id)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (t) => {
          this.trip.set(t);
          this.loadedForId = id;
          if (this.mode() === 'edit') this.seedEditBuffer();
          if (this.mode() === 'pack') this.seedPackedMap();
        },
        error: (e) => {
          this.error.set(e?.error || e?.message || 'Failed to load trip');
          this.trip.set(null);
          this.loadedForId = null;
        },
      });
  }

  saveTrip() {
    if (!this.canSave(this.edit)) return;
    const dto = new TripEditDto({
      id: this.edit!.id,
      startDate: new Date(this.edit!.startDate),
      endDate: new Date(this.edit!.endDate),
      destination: (this.edit!.destination ?? '').trim(),
    });

    this.loading.set(true);
    this.error.set('');
    this.tripsService
      .update(dto)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => this.loadTrip(dto.id),
        error: (e) => this.error.set(e?.error || e?.message || 'Failed to save trip'),
      });
  }

  // --- PACK: checkbox change -> call API (optimistic, rollback on error) ---
  onPackedChange(it: TripCatalogItemReadDto, checked: boolean) {
    if (it.tripItemId == null || this.loadedForId == null) return;

    // optimistic update
    const prev = this.packedMap()[it.tripItemId] ?? false;
    const map = { ...this.packedMap() };
    map[it.tripItemId] = checked;
    this.packedMap.set(map);

    // also reflect on trip() for the disabled checkboxes in edit mode
    this.mutateTripItem(it.tripItemId, { isPacked: checked });

    const tripItemEditDto = new TripItemEditDto({
      id: it.tripItemId,
      isPacked: checked
    });
    this.tripsService.updateTripItem(this.loadedForId, it.tripItemId, tripItemEditDto).subscribe({
      next: () => { /* success, keep optimistic state */ },
      error: (e) => {
        // rollback
        const rollback = { ...this.packedMap() };
        rollback[it.tripItemId!] = prev;
        this.packedMap.set(rollback);
        this.mutateTripItem(it.tripItemId!, { isPacked: prev });
        this.error.set(e?.error || e?.message || 'Failed to update packed status');
      },
    });
  }

  // --- EDIT: add/remove items -> call API then refresh ---
  addToTrip(it: TripCatalogItemReadDto) {
    if (this.loadedForId == null) return;
    const dto = new TripItemCreateDto({ itemId: it.itemId, isPacked: false });
    this.loading.set(true);
    this.error.set('');
    this.tripsService
      .addTripItem(this.loadedForId, dto)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => this.loadTrip(this.loadedForId!),
        error: (e) => this.error.set(e?.error || e?.message || 'Failed to add item'),
      });
  }

  removeFromTrip(it: TripCatalogItemReadDto) {
    if (this.loadedForId == null || it.tripItemId == null) return;
    this.loading.set(true);
    this.error.set('');
    this.tripsService
      .deleteTripItem(this.loadedForId, it.tripItemId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => this.loadTrip(this.loadedForId!),
        error: (e) => this.error.set(e?.error || e?.message || 'Failed to remove item'),
      });
  }

  // --- small helper to update a single item inside trip() ---
  private mutateTripItem(tripItemId: number, patch: Partial<TripCatalogItemReadDto>) {
    const t = this.trip();
    if (!t) return;
    const items = t.items.slice();
    const idx = items.findIndex(x => x.tripItemId === tripItemId);
    if (idx >= 0) {
      Object.assign(items[idx], patch);
      const updatedTrip = new TripDetailsReadDto({ ...t, items });
      this.trip.set(updatedTrip);
    }
  }

  // helpers
  private toYyyyMmDd(s: string | undefined): string {
    if (!s) return '';
    return s.length >= 10 ? s.slice(0, 10) : s;
  }
  private formatDateToInput(date: Date | undefined): string {
    if (!date) return '';
    const d = new Date(date);
    return d.toISOString().slice(0, 10);
  }

  canSave(editBuffer: typeof this.edit): boolean {
    if (!editBuffer || !editBuffer.destination) return false;
    if (!editBuffer.startDate || !editBuffer.endDate) return false;
    return editBuffer.startDate <= editBuffer.endDate;
  }
}
