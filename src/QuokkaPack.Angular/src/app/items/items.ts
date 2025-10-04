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
      map.set(c.id, { key: c.id, category: c, items: [] });
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

    // load categories first (select options), then items
    this.categoriesSvc.list().subscribe({
      next: cats => this.categories.set(cats),
      error: e => this.error.set(e?.error || e?.message || 'Failed to load categories'),
      complete: () => {
        this.itemsSvc.list().subscribe({
          next: items => this.items.set(items),
          error: e => this.error.set(e?.error || e?.message || 'Failed to load items'),
          complete: () => this.loading.set(false),
        });
      },
    });
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

  removeCategory(catId: number) {
    if (!confirm('Delete this category?')) return;
    this.categoriesSvc.remove(catId).subscribe({
      next: () => this.categories.set(this.categories().filter(c => c.id !== catId)),
      error: e => this.error.set(e?.error || e?.message || 'Failed to delete category'),
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

  removeItem(itemId: number) {
    if (!confirm('Delete this item?')) return;
    this.itemsSvc.remove(itemId).subscribe({
      next: () => this.items.set(this.items().filter(i => i.id !== itemId)),
      error: e => this.error.set(e?.error || e?.message || 'Failed to delete item'),
    });
  }

  badgeText(g: CategoryGroup) {
    return g.category.isDefault ? 'Default' : 'Optional';
  }
}
