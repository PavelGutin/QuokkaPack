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

  // --- Edit buffer ---
  edit: TripEditDto | null = null;

  // --- Pack mode local state: tripItemId -> packed? ---
  packedMap = signal<Record<number, boolean>>({});

  // All items grouped by category (for EDIT mode)
  groups = computed(() => {
    const t = this.trip();
    const by = new Map<string, TripCatalogItemReadDto[]>();
    for (const it of t?.items ?? []) {
      const key = it.categoryName ?? '(Uncategorized)';
      if (!by.has(key)) by.set(key, []);
      by.get(key)!.push(it);
    }
    return Array.from(by.entries())
      .sort((a, b) => a[0].localeCompare(b[0]))
      .map(([category, items]) => ({
        category,
        items: items.slice().sort((a, b) => a.name.localeCompare(b.name)),
      }));
  });

  // Only items that are part of the trip (for PACK mode)
  packGroups = computed(() => {
    const t = this.trip();
    const by = new Map<string, TripCatalogItemReadDto[]>();
    for (const it of t?.items ?? []) {
      if (it.tripItemId == null) continue;
      const key = it.categoryName ?? '(Uncategorized)';
      if (!by.has(key)) by.set(key, []);
      by.get(key)!.push(it);
    }
    return Array.from(by.entries())
      .sort((a, b) => a[0].localeCompare(b[0]))
      .map(([category, items]) => ({
        category,
        items: items.slice().sort((a, b) => a.name.localeCompare(b.name)),
      }));
  });

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
      startDate: (t.startDate ?? '').slice(0, 10),
      endDate: (t.endDate ?? '').slice(0, 10),
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
    if (!this.edit) return;
    const dto: TripEditDto = {
      id: this.edit.id,
      startDate: this.toYyyyMmDd(this.edit.startDate),
      endDate: this.toYyyyMmDd(this.edit.endDate),
      destination: (this.edit.destination ?? '').trim(),
    };
    if (!this.canSave(dto)) return;

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

    const tripItemEditDto: TripItemEditDto = {
      id: it.tripItemId,
      isPacked: checked
    };
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
    const dto: TripItemCreateDto = { itemId: it.itemId, isPacked: false };
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
      items[idx] = { ...items[idx], ...patch };
      this.trip.set({ ...t, items });
    }
  }

  // helpers
  private toYyyyMmDd(s: string | undefined): string {
    if (!s) return '';
    return s.length >= 10 ? s.slice(0, 10) : s;
  }
  private isYyyyMmDd(s: string): boolean {
    return /^\d{4}-\d{2}-\d{2}$/.test(s);
  }
  canSave(dto: TripEditDto): boolean {
    if (!dto.destination) return false;
    if (!this.isYyyyMmDd(dto.startDate) || !this.isYyyyMmDd(dto.endDate)) return false;
    return dto.startDate <= dto.endDate;
  }
}
