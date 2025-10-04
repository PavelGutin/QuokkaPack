import { Injectable, inject } from '@angular/core';
import { Observable, forkJoin } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { TripsService } from './trips.service';
import {
  TripDetailsReadDto,
  TripItemReadDto,
  ItemReadDto,
  CategoryReadDto
} from '../../models/api-types';

/**
 * View model for editing a trip with catalog data
 */
export interface TripEditViewModel {
  trip: TripDetailsReadDto;
  catalog: ItemReadDto[];
  categories: CategoryReadDto[];
  groupedByCategory: CategoryGroup[];
}

/**
 * Items grouped by category with trip status
 */
export interface CategoryGroup {
  categoryId: number;
  categoryName: string;
  itemsInTrip: ItemWithTripStatus[];
  itemsNotInTrip: ItemReadDto[];
}

/**
 * Catalog item enhanced with trip status
 */
export interface ItemWithTripStatus {
  id: number;
  name: string;
  categoryId: number;
  categoryName: string;
  tripItemId: number;
  isPacked: boolean;
}

@Injectable({ providedIn: 'root' })
export class TripJoinService {
  private tripsSvc = inject(TripsService);

  // Cached catalog - reused across trip views
  private catalogCache$ = this.tripsSvc.listAllItems().pipe(
    shareReplay({ bufferSize: 1, refCount: true })
  );

  private categoriesCache$ = this.tripsSvc.listAllCategories().pipe(
    shareReplay({ bufferSize: 1, refCount: true })
  );

  /**
   * Get trip with catalog for editing/packing view
   * Performs client-side join of trip items with full catalog
   */
  getTripEditView(tripId: number): Observable<TripEditViewModel> {
    return forkJoin({
      trip: this.tripsSvc.get(tripId),
      catalog: this.catalogCache$,
      categories: this.categoriesCache$
    }).pipe(
      map(({ trip, catalog, categories }) =>
        this.buildEditView(trip, catalog, categories)
      )
    );
  }

  /**
   * Clear catalog cache (use after adding/editing catalog items)
   */
  clearCache(): void {
    this.catalogCache$ = this.tripsSvc.listAllItems().pipe(
      shareReplay({ bufferSize: 1, refCount: true })
    );
    this.categoriesCache$ = this.tripsSvc.listAllCategories().pipe(
      shareReplay({ bufferSize: 1, refCount: true })
    );
  }

  private buildEditView(
    trip: TripDetailsReadDto,
    catalog: ItemReadDto[],
    categories: CategoryReadDto[]
  ): TripEditViewModel {
    // Create lookup map for fast joining
    const tripItemsMap = new Map<number, TripItemReadDto>(
      trip.items.map(ti => [ti.itemId, ti])
    );

    // Group catalog by category
    const groupedByCategory = this.groupByCategory(catalog, tripItemsMap);

    return {
      trip,
      catalog,
      categories,
      groupedByCategory
    };
  }

  private groupByCategory(
    catalog: ItemReadDto[],
    tripItemsMap: Map<number, TripItemReadDto>
  ): CategoryGroup[] {
    const byCategory = new Map<number, CategoryGroup>();

    for (const item of catalog) {
      // Initialize category group if needed
      if (!byCategory.has(item.categoryId)) {
        byCategory.set(item.categoryId, {
          categoryId: item.categoryId,
          categoryName: item.categoryName,
          itemsInTrip: [],
          itemsNotInTrip: []
        });
      }

      const group = byCategory.get(item.categoryId)!;
      const tripItem = tripItemsMap.get(item.id);

      if (tripItem) {
        // Item is in the trip - enhance with trip data
        group.itemsInTrip.push({
          ...item,
          tripItemId: tripItem.tripItemId,
          isPacked: tripItem.isPacked
        });
      } else {
        // Item is available but not in trip
        group.itemsNotInTrip.push(item);
      }
    }

    // Return sorted by category name
    return Array.from(byCategory.values())
      .sort((a, b) => a.categoryName.localeCompare(b.categoryName));
  }
}
