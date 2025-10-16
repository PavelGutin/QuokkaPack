// src/app/items/items.component.ts
import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';

import { ItemsService } from '../core/features/items/items.service';
import { CategoriesService } from '../core/features/categories/categories.service';

import { switchMap } from 'rxjs';

import {
  ItemReadDto,
  ItemCreateDto,
  CategoryReadDto,
  CategoryCreateDto,
} from '../core/models/api-types';

type CategoryKey = number;
interface CategoryGroup {
  key: CategoryKey;
  category: CategoryReadDto;
  items: ItemReadDto[];
}

@Component({
  selector: 'app-items',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './items.html',
  styleUrls: ['./items.scss'],
})
export class ItemsComponent {
  private itemsSvc = inject(ItemsService);
  private categoriesSvc = inject(CategoriesService);
  private fb = inject(FormBuilder);

  // data
  items = signal<ItemReadDto[]>([]);
  categories = signal<CategoryReadDto[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  showArchived = signal(false);

  // forms
  addCategoryForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    isDefault: [false],
  });

  addItemForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    categoryId: [null as number | null, [Validators.required]],
  });

  // derived
  itemCount = computed(() => this.items().length);

  grouped = computed<CategoryGroup[]>(() => {
    const cats = this.categories();
    const map = new Map<CategoryKey, CategoryGroup>();

    // seed groups so empty categories still render
    for (const c of cats) {
      map.set(c.id!, { key: c.id!, category: c, items: [] });
    }

    // place each item into its category group
    for (const it of this.items()) {
      const key: CategoryKey =
        it.categoryId ??
        -1;                                // bucket for unknowns (or skip)

      if (!map.has(key)) {
        const fromList = cats.find(c => c.id === key);
        const unknownCategory = new CategoryReadDto({ id: key, name: '(Unknown)', isDefault: false });
        map.set(key, {
          key,
          category: fromList ?? unknownCategory,
          items: []
        });
      }
      map.get(key)!.items.push(it);
    }

    // sort items within each group
    for (const g of map.values()) {
      g.items.sort((a, b) => (a.name ?? '').localeCompare(b.name ?? ''));
    }

    // order groups: Default first, then alphabetical by name
    const arr = Array.from(map.values());
    arr.sort((a, b) => {
      const ad = a.category.isDefault ? 0 : 1;
      const bd = b.category.isDefault ? 0 : 1;
      if (ad !== bd) return ad - bd;
      return (a.category.name ?? '').localeCompare(b.category.name ?? '');
    });

    return arr;
  });

  constructor() {
    this.loadAll();
  }

  private loadAll() {
    this.loading.set(true);
    this.error.set(null);

    const includeArchived = this.showArchived();

    // load categories first (select options), then items
    this.categoriesSvc.list(includeArchived).subscribe({
      next: cats => this.categories.set(cats),
      error: e => this.error.set(e?.error || e?.message || 'Failed to load categories'),
      complete: () => {
        this.itemsSvc.list(includeArchived).subscribe({
          next: items => this.items.set(items),
          error: e => this.error.set(e?.error || e?.message || 'Failed to load items'),
          complete: () => this.loading.set(false),
        });
      },
    });
  }

  toggleShowArchived() {
    this.showArchived.update(val => !val);
    this.loadAll();
  }

  // --- category ops ---
  addCategory() {
    if (this.addCategoryForm.invalid) return;
    const dto = this.addCategoryForm.value as unknown as CategoryCreateDto;
    this.categoriesSvc.create(dto).subscribe({
      next: created => {
        this.categories.set([...this.categories(), created]);
        this.addCategoryForm.reset({ name: '', isDefault: false });
      },
      error: e => this.error.set(e?.error || e?.message || 'Failed to add category'),
    });
  }

  archiveCategory(catId: number) {
    if (!confirm('Archive this category and all its items?')) return;
    this.categoriesSvc.archive(catId).subscribe({
      next: () => this.categories.set(this.categories().filter(c => c.id !== catId)),
      error: e => {
        // Handle structured error response with trip information
        if (e?.error?.trips && Array.isArray(e.error.trips)) {
          const tripLinks = e.error.trips
            .map((trip: any) => `<a href="/trips/${trip.id}/pack" class="alert-link">${trip.destination}</a>`)
            .join(', ');
          const message = e.error.message || 'Cannot archive category';
          // Replace the plain trip list with clickable links
          const enhancedMessage = message.replace(/Remove them from trips first: .*$/, `Remove them from trips first: ${tripLinks}`);
          this.error.set(enhancedMessage);
        } else {
          const errorMsg = e?.error?.message || e?.error || e?.message || 'Failed to archive category';
          this.error.set(errorMsg);
        }
      },
    });
  }

  restoreCategory(catId: number) {
    this.categoriesSvc.restore(catId).subscribe({
      next: () => {
        // Reload to get updated data
        this.loadAll();
      },
      error: e => this.error.set(e?.error || e?.message || 'Failed to restore category'),
    });
  }

  removeCategory(catId: number) {
    if (!confirm('Permanently delete this category? This cannot be undone.')) return;
    this.categoriesSvc.remove(catId).subscribe({
      next: () => this.categories.set(this.categories().filter(c => c.id !== catId)),
      error: e => {
        const errorMsg = e?.error || e?.message || 'Failed to delete category';
        this.error.set(errorMsg);
      },
    });
  }

  // --- item ops ---
  addItem() {
    if (this.addItemForm.invalid) return;
    const dto = this.addItemForm.value as ItemCreateDto;

    this.itemsSvc.create(dto).subscribe({
      next: (created) => {
        // enforce contract (fail fast if server didn't include categoryId and categoryName)
        if (!created?.id || !created?.categoryId) {
          throw new Error('API contract: expected ItemReadDto with categoryId and categoryName.');
        }
        this.items.update(prev => [created, ...prev]);
        this.addItemForm.reset({ name: '', categoryId: null });
      },
      error: e => this.error.set(e?.error || e?.message || 'Failed to add item'),
    });
  }

  archiveItem(itemId: number) {
    if (!confirm('Archive this item?')) return;
    this.itemsSvc.archive(itemId).subscribe({
      next: () => this.items.set(this.items().filter(i => i.id !== itemId)),
      error: e => {
        // Handle structured error response with trip information
        if (e?.error?.trips && Array.isArray(e.error.trips)) {
          const tripLinks = e.error.trips
            .map((trip: any) => `<a href="/trips/${trip.id}/pack" class="alert-link">${trip.destination}</a>`)
            .join(', ');
          const message = e.error.message || 'Cannot archive item that is in use';
          // Replace the plain trip list with clickable links
          const enhancedMessage = message.replace(/Remove it from these trips first: .*$/, `Remove it from these trips first: ${tripLinks}`);
          this.error.set(enhancedMessage);
        } else {
          const errorMsg = e?.error?.message || e?.error || e?.message || 'Failed to archive item';
          this.error.set(errorMsg);
        }
      },
    });
  }

  restoreItem(itemId: number) {
    this.itemsSvc.restore(itemId).subscribe({
      next: () => {
        // Reload to get updated data
        this.loadAll();
      },
      error: e => this.error.set(e?.error || e?.message || 'Failed to restore item'),
    });
  }

  removeItem(itemId: number) {
    if (!confirm('Permanently delete this item? This cannot be undone.')) return;
    this.itemsSvc.remove(itemId).subscribe({
      next: () => this.items.set(this.items().filter(i => i.id !== itemId)),
      error: e => {
        // Handle structured error response with trip information
        if (e?.error?.trips && Array.isArray(e.error.trips)) {
          const tripLinks = e.error.trips
            .map((trip: any) => `<a href="/trips/${trip.id}/pack" class="alert-link">${trip.destination}</a>`)
            .join(', ');
          this.error.set(`Cannot delete item that is used in trips: ${tripLinks}`);
        } else {
          const errorMsg = e?.error?.message || e?.error || e?.message || 'Failed to delete item';
          this.error.set(errorMsg);
        }
      },
    });
  }

  badgeText(g: CategoryGroup) {
    return g.category.isDefault ? 'Default' : 'Optional';
  }
}
