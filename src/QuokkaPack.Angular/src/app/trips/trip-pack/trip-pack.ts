import { Component, OnInit, inject, effect, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TripsService } from '../../core/features/trips/trips.service';

/** ——— DTOs inferred from your Razor code ——— */
export interface CategoryDto {
  id: number;
  name: string;
}

export interface ItemReadDto {
  id: number;
  name: string;
  category: CategoryDto;
}

export interface TripItemReadDto {
  id: number;
  itemReadDto: ItemReadDto;
  quantity: number;
  isPacked: boolean;
  notes?: string | null;
}

export interface TripItemEditDto {
  id: number;
  isPacked: boolean;
}

/** Trip brief (already in your models/trip.ts but we keep a light copy here) */
export interface TripReadDtoLite {
  id: number;
  destination: string;
  startDate: string;
  endDate: string;
}

/** View-model used by the template */
export type TripItemVM = {
  id: number;
  name: string;
  category: string;
  quantity: number;
  isPacked: boolean;
  /** mark rows changed so we only send minimal batch payload */
  dirty: boolean;
};

@Component({
  standalone: true,
  selector: 'app-trip-pack',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './trip-pack.html',
  styleUrls: ['./trip-pack.scss'],
})
export class TripPackComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private trips = inject(TripsService);

  tripId = signal<number | null>(null);
  loading = signal(true);
  saving = signal(false);
  error = signal<string>('');

  // raw/loaders
  trip = signal<TripReadDtoLite | null>(null);
  items = signal<TripItemVM[]>([]);

  // simple text filter (optional)
  filter = signal<string>('');

  // derived: group items by category name, sorted by category
  grouped = computed(() => {
    const f = this.filter().trim().toLowerCase();
    const filtered = f
      ? this.items().filter(i => i.name.toLowerCase().includes(f))
      : this.items();

    const map = new Map<string, TripItemVM[]>();
    for (const it of filtered) {
      if (!map.has(it.category)) map.set(it.category, []);
      map.get(it.category)!.push(it);
    }
    // sort categories + items by name
    const entries = [...map.entries()]
      .sort((a, b) => a[0].localeCompare(b[0]))
      .map(([cat, arr]) => [cat, arr.sort((x, y) => x.name.localeCompare(y.name))] as const);
    return entries;
  });

  // add a backing property
  get filterText(): string {
    return this.filter();
  }
  set filterText(value: string) {
    this.filter.set(value);
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

    // load trip + items in parallel
    this.trips.get(id).subscribe({
      next: (t) => {
        // keep a very small shape for header display
        this.trip.set({
          id: Number(t.id),
          destination: t.destination,
          startDate: (t as any).startDate?.toString?.() ?? t['startDate'] ?? '',
          endDate: (t as any).endDate?.toString?.() ?? t['endDate'] ?? '',
        });
      },
      error: (e) => {
        this.error.set(e?.error || e?.message || 'Failed to load trip');
      },
    });

    this.trips.getItems(id).subscribe({
      next: (dtos: TripItemReadDto[]) => {
        const vms: TripItemVM[] = (dtos ?? []).map(d => ({
          id: d.id,
          name: d.itemReadDto?.name ?? '(Unnamed)',
          category: d.itemReadDto?.category?.name ?? '(Uncategorized)',
          quantity: d.quantity ?? 1,
          isPacked: !!d.isPacked,
          dirty: false,
        }));
        this.items.set(vms);
      },
      error: (e) => this.error.set(e?.error || e?.message || 'Failed to load items'),
      complete: () => this.loading.set(false),
    });
  }

  togglePacked(item: TripItemVM, checked: boolean) {
    item.isPacked = checked;
    item.dirty = true;
    // trigger signals update (replace array instance)
    this.items.set([...this.items()]);
  }

  toggleCategory(cat: string, packed: boolean) {
    const updated = this.items().map(i => {
      if (i.category === cat) {
        return { ...i, isPacked: packed, dirty: true };
      }
      return i;
    });
    this.items.set(updated);
  }

  anyDirty = computed(() => this.items().some(i => i.dirty));

  save() {
    const id = this.tripId();
    if (!id) return;

    const payload: TripItemEditDto[] = this.items()
      .filter(i => i.dirty)
      .map(i => ({ id: i.id, isPacked: i.isPacked }));

    if (payload.length === 0) return;

    this.saving.set(true);
    this.error.set('');

    this.trips.updatePackedStatus(id, payload).subscribe({
      next: () => {
        // clear dirty flags after successful save
        this.items.set(this.items().map(i => ({ ...i, dirty: false })));
      },
      error: (e) => this.error.set(e?.error || e?.message || 'Failed to update packed status'),
      complete: () => this.saving.set(false),
    });
  }
}
