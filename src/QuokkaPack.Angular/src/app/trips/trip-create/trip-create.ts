import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TripsService } from '../../core/features/trips/trips.service';
import { ItemsService } from '../../core/features/items/items.service';
import { TripCreateDto, CategoryReadDto, ItemReadDto } from '../../core/models/api-types';

@Component({
  selector: 'app-trip-create',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './trip-create.html',
  styleUrl: './trip-create.scss'
})
export class TripCreate implements OnInit {
  private tripsSvc = inject(TripsService);
  private itemsSvc = inject(ItemsService);
  private router = inject(Router);

  trip: TripCreateDto = new TripCreateDto({
    startDate: new Date(),
    endDate: new Date(),
    destination: '',
    categoryIds: []
  });

  // String versions for the date inputs
  startDateStr = '';
  endDateStr = '';

  allCategories: CategoryReadDto[] = [];
  allItems: ItemReadDto[] = [];
  selectedCategoryIds: number[] = [];
  loading = false;
  error = '';

  ngOnInit(): void {
    this.loadCategories();
  }

  private loadCategories(): void {
    this.loading = true;
    this.tripsSvc.listAllCategories().subscribe({
      next: (categories) => {
        this.allCategories = categories;
        // Pre-select default categories
        this.selectedCategoryIds = categories
          .filter(c => c.isDefault)
          .map(c => c.id);
        this.loadItems();
      },
      error: (err) => {
        this.error = 'Failed to load categories';
        this.loading = false;
      }
    });
  }

  private loadItems(): void {
    this.itemsSvc.list().subscribe({
      next: (items) => {
        this.allItems = items;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load items';
        this.loading = false;
      }
    });
  }

  isCategorySelected(categoryId: number): boolean {
    return this.selectedCategoryIds.includes(categoryId);
  }

  toggleCategory(categoryId: number): void {
    const index = this.selectedCategoryIds.indexOf(categoryId);
    if (index > -1) {
      this.selectedCategoryIds.splice(index, 1);
    } else {
      this.selectedCategoryIds.push(categoryId);
    }
  }

  onSubmit(): void {
    if (!this.trip.destination || !this.startDateStr || !this.endDateStr) {
      this.error = 'Please fill in all required fields';
      return;
    }

    // Convert string dates to Date objects
    this.trip.startDate = new Date(this.startDateStr);
    this.trip.endDate = new Date(this.endDateStr);
    this.trip.categoryIds = this.selectedCategoryIds;
    this.loading = true;
    this.error = '';

    this.tripsSvc.create(this.trip).subscribe({
      next: (response: any) => {
        this.loading = false;
        // Navigate to edit page (matching Razor behavior)
        this.router.navigate(['/trips', response.id, 'edit']);
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message || 'Failed to create trip';
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/trips']);
  }

  getItemsForCategory(categoryId: number): ItemReadDto[] {
    return this.allItems.filter(item => item.categoryId === categoryId);
  }
}
