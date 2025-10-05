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
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { TripsService } from '../../core/features/trips/trips.service';
import { TripJoinService, CategoryGroup, ItemWithTripStatus } from '../../core/features/trips/trip-join.service';
import {
  TripDetailsReadDto,
  TripEditDto,
  ItemReadDto,
  TripItemCreateDto,
  TripItemEditDto
} from '../../core/models/api-types';

type Group = CategoryGroup;

@Component({
  selector: 'app-trip-pack',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './trip-pack.html',
  styleUrl: './trip-pack.scss'
})

export class TripPack implements OnInit, OnChanges {
  private tripsService = inject(TripsService);
  private tripJoinService = inject(TripJoinService);
  private route = inject(ActivatedRoute);

  @Input({ required: false }) tripId?: number;

  mode = signal<'pack' | 'edit'>('pack');
  trip = signal<TripDetailsReadDto | null>(null);
  groups = signal<Group[]>([]);
  loading = signal<boolean>(false);
  error = signal<string>('');
  private loadedForId: number | null = null;

  // --- Edit buffer (uses string dates for form inputs) ---
  edit: { id: number; startDate: string; endDate: string; destination: string } | null = null;

  // --- Pack mode local state: tripItemId -> packed? ---
  packedMap = signal<Record<number, boolean>>({});

  // Groups with items in trip (for pack mode)
  packGroups = computed<Group[]>(() =>
    this.groups().filter(g => g.itemsInTrip.length > 0)
  );

  // Groups with items in trip (for edit mode)
  groupsInTrip = computed<Group[]>(() =>
    this.groups().filter(g => g.itemsInTrip.length > 0)
  );

  // Groups where NO items have been added to trip yet (completely unused categories)
  availableGroups = computed<Group[]>(() =>
    this.groups().filter(g => g.itemsInTrip.length === 0 && g.itemsNotInTrip.length > 0)
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
    this.mode.set(m);
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
    for (const group of this.groups()) {
      for (const item of group.itemsInTrip) {
        map[item.tripItemId] = item.isPacked;
      }
    }
    this.packedMap.set(map);
  }

  private loadTrip(id: number) {
    this.loading.set(true);
    this.error.set('');
    this.tripJoinService
      .getTripEditView(id)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (viewModel) => {
          this.trip.set(viewModel.trip);
          this.groups.set(viewModel.groupedByCategory);
          this.loadedForId = id;
          if (this.mode() === 'edit') this.seedEditBuffer();
          if (this.mode() === 'pack') this.seedPackedMap();
        },
        error: (e) => {
          this.error.set(e?.error || e?.message || 'Failed to load trip');
          this.trip.set(null);
          this.groups.set([]);
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
  onPackedChange(item: ItemWithTripStatus, checked: boolean) {
    if (this.loadedForId == null) return;

    // optimistic update
    const prev = item.isPacked;
    item.isPacked = checked;

    const map = { ...this.packedMap() };
    map[item.tripItemId] = checked;
    this.packedMap.set(map);

    const tripItemEditDto = new TripItemEditDto({
      id: item.tripItemId,
      isPacked: checked
    });

    this.tripsService.updateTripItem(this.loadedForId, item.tripItemId, tripItemEditDto).subscribe({
      next: () => { /* success, keep optimistic state */ },
      error: (e) => {
        // rollback
        item.isPacked = prev;
        const rollback = { ...this.packedMap() };
        rollback[item.tripItemId] = prev;
        this.packedMap.set(rollback);
        this.error.set(e?.error || e?.message || 'Failed to update packed status');
      },
    });
  }

  // --- EDIT: add/remove items -> call API then refresh ---
  addToTrip(item: ItemReadDto) {
    if (this.loadedForId == null) return;
    const dto = new TripItemCreateDto({ itemId: item.id, isPacked: false });
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

  removeFromTrip(item: ItemWithTripStatus) {
    if (this.loadedForId == null) return;
    this.loading.set(true);
    this.error.set('');
    this.tripsService
      .deleteTripItem(this.loadedForId, item.tripItemId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => this.loadTrip(this.loadedForId!),
        error: (e) => this.error.set(e?.error || e?.message || 'Failed to remove item'),
      });
  }

  addCategoryToTrip(group: Group) {
    if (this.loadedForId == null || group.itemsNotInTrip.length === 0) return;
    this.loading.set(true);
    this.error.set('');

    // Create add requests for all items in the category
    const addRequests = group.itemsNotInTrip.map(item => {
      const dto = new TripItemCreateDto({ itemId: item.id, isPacked: false });
      return this.tripsService.addTripItem(this.loadedForId!, dto);
    });

    // Execute all requests in parallel
    forkJoin(addRequests)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => this.loadTrip(this.loadedForId!),
        error: (e) => this.error.set(e?.error || e?.message || 'Failed to add category'),
      });
  }

  // helpers
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

  getItemNames(group: Group): string {
    return group.itemsNotInTrip.map(it => it.name).join(', ');
  }
}
