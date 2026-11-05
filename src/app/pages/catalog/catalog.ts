import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Category, Item } from '../../data/model';
import * as ops from '../../data/ops';
import { Store } from '../../data/store';
import { ClickOutside } from '../../ui/click-outside';
import { InlineEdit } from '../../ui/inline-edit';

@Component({
  selector: 'qp-catalog',
  imports: [FormsModule, InlineEdit, ClickOutside],
  templateUrl: './catalog.html',
})
export class CatalogPage {
  store = inject(Store);
  query = signal('');
  showArchived = signal(false);
  newCategory = '';
  openMenu = signal<string | null>(null);

  archivedCount = computed(
    () => this.store.categories().filter((c) => c.archived).length + this.store.items().filter((i) => i.archived).length,
  );

  /** How many trips use each item. */
  usage = computed(() => {
    const m = new Map<string, number>();
    for (const t of this.store.trips()) for (const ti of t.items) m.set(ti.itemId, (m.get(ti.itemId) ?? 0) + 1);
    return m;
  });

  groups = computed(() => {
    const q = this.query().trim().toLocaleLowerCase();
    const arch = this.showArchived();
    const items = this.store.items();
    return [...this.store.categories()]
      .filter((c) => arch || !c.archived)
      .sort(ops.byName)
      .map((category) => {
        const catHit = !q || category.name.toLocaleLowerCase().includes(q);
        const list = items
          .filter((i) => i.categoryId === category.id && (arch || !i.archived))
          .filter((i) => catHit || i.name.toLocaleLowerCase().includes(q))
          .sort(ops.byName);
        return { category, items: list, visible: catHit || list.length > 0 };
      })
      .filter((g) => g.visible);
  });

  activeCategories = computed(() => this.store.categories().filter((c) => !c.archived).sort(ops.byName));

  addCategory() {
    if (!this.newCategory.trim()) return;
    if (this.store.update((d) => ops.addCategory(d, this.newCategory).doc)) this.newCategory = '';
  }

  renameCategory(c: Category, name: string) {
    this.store.update((d) => ops.renameCategory(d, c.id, name));
  }

  archiveCategory(c: Category, archived: boolean) {
    this.store.update((d) => ops.setCategoryArchived(d, c.id, archived), archived ? `Archived “${c.name}”` : undefined);
  }

  deleteCategory(c: Category, count: number) {
    this.store.update((d) => ops.deleteCategory(d, c.id), `Deleted “${c.name}”${count ? ` and ${count} item${count === 1 ? '' : 's'}` : ''}`);
  }

  addItem(c: Category, box: HTMLInputElement) {
    if (!box.value.trim()) return;
    if (this.store.update((d) => ops.addItem(d, c.id, box.value).doc)) box.value = '';
  }

  renameItem(i: Item, name: string) {
    this.store.update((d) => ops.updateItem(d, i.id, { name }));
  }

  moveItem(i: Item, categoryId: string) {
    this.openMenu.set(null);
    const cat = this.store.categories().find((c) => c.id === categoryId);
    this.store.update((d) => ops.updateItem(d, i.id, { categoryId }), `Moved “${i.name}” to ${cat?.name}`);
  }

  archiveItem(i: Item, archived: boolean) {
    this.openMenu.set(null);
    this.store.update((d) => ops.setItemArchived(d, i.id, archived), archived ? `Archived “${i.name}”` : undefined);
  }

  deleteItem(i: Item) {
    this.openMenu.set(null);
    this.store.update((d) => ops.deleteItem(d, i.id), `Deleted “${i.name}”`);
  }
}
