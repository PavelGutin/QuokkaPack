import { describe, expect, it } from 'vitest';
import { emptyDoc } from './model';
import * as ops from './ops';

function seeded() {
  let doc = emptyDoc();
  const a = ops.addCategory(doc, 'Toiletries');
  doc = a.doc;
  const b = ops.addCategory(doc, 'Clothing');
  doc = b.doc;
  const tb = ops.addItem(doc, a.id, 'Toothbrush');
  doc = tb.doc;
  const tp = ops.addItem(doc, a.id, 'Toothpaste');
  doc = tp.doc;
  const sk = ops.addItem(doc, b.id, 'Socks');
  doc = sk.doc;
  return { doc, toiletries: a.id, clothing: b.id, toothbrush: tb.id, toothpaste: tp.id, socks: sk.id };
}

describe('catalog', () => {
  it('dedupes categories and items case-insensitively', () => {
    const s = seeded();
    expect(ops.addCategory(s.doc, '  toiletries ').id).toBe(s.toiletries);
    expect(ops.addItem(s.doc, s.toiletries, 'TOOTHBRUSH').id).toBe(s.toothbrush);
    expect(s.doc.categories).toHaveLength(2);
  });

  it('rejects renaming onto an existing name', () => {
    const s = seeded();
    expect(() => ops.renameCategory(s.doc, s.clothing, 'toiletries')).toThrow(/already exists/);
  });

  it('does not mutate the input', () => {
    const s = seeded();
    const snap = JSON.stringify(s.doc);
    ops.deleteCategory(s.doc, s.toiletries);
    expect(JSON.stringify(s.doc)).toBe(snap);
  });

  it('deleting a category removes its items from trips', () => {
    const s = seeded();
    const t = ops.createTrip(s.doc, { name: 'Paris' }, { categoryIds: [s.toiletries, s.clothing] });
    const doc = ops.deleteCategory(t.doc, s.toiletries);
    expect(doc.items.map((i) => i.name)).toEqual(['Socks']);
    expect(doc.trips[0].items.map((i) => i.itemId)).toEqual([s.socks]);
  });

  it('restoring an archived item restores its category', () => {
    const s = seeded();
    let doc = ops.setCategoryArchived(s.doc, s.toiletries, true);
    doc = ops.setItemArchived(doc, s.toothbrush, true);
    doc = ops.setItemArchived(doc, s.toothbrush, false);
    expect(doc.categories.find((c) => c.id === s.toiletries)!.archived).toBe(false);
  });
});

describe('trips', () => {
  it('creates from categories, skipping archived items', () => {
    const s = seeded();
    const doc = ops.setItemArchived(s.doc, s.toothpaste, true);
    const t = ops.createTrip(doc, { name: 'Rome' }, { categoryIds: [s.toiletries] });
    expect(t.doc.trips[0].items.map((i) => i.itemId)).toEqual([s.toothbrush]);
  });

  it('validates dates', () => {
    expect(() => ops.createTrip(emptyDoc(), { name: 'x', startDate: '2026-05-02', endDate: '2026-05-01' })).toThrow();
    expect(() => ops.createTrip(emptyDoc(), { name: '  ' })).toThrow();
  });

  it('duplicates with quantities but unpacked', () => {
    const s = seeded();
    let { doc, id } = ops.createTrip(s.doc, { name: 'Rome' }, { categoryIds: [s.clothing] });
    const ti = doc.trips[0].items[0];
    doc = ops.setQuantity(doc, id, ti.id, 5);
    doc = ops.setPacked(doc, id, [ti.id], true);
    const dup = ops.duplicateTrip(doc, id);
    const copy = dup.doc.trips.find((t) => t.id === dup.id)!;
    expect(copy.name).toBe('Rome (copy)');
    expect(copy.items[0]).toMatchObject({ itemId: s.socks, quantity: 5, packed: false });
  });

  it('quick-adds a new catalog item onto a trip', () => {
    const s = seeded();
    const t = ops.createTrip(s.doc, { name: 'Rome' });
    const doc = ops.quickAddToTrip(t.doc, t.id, s.clothing, 'Hat');
    expect(doc.items.some((i) => i.name === 'Hat')).toBe(true);
    expect(ops.tripGroups(doc, doc.trips[0]).map((g) => g.category.name)).toEqual(['Clothing']);
  });

  it('computes stats and groups', () => {
    const s = seeded();
    let { doc, id } = ops.createTrip(s.doc, { name: 'Rome' }, { categoryIds: [s.toiletries, s.clothing] });
    const trip = doc.trips[0];
    doc = ops.setPacked(doc, id, [trip.items[0].id], true);
    expect(ops.tripStats(doc.trips[0])).toEqual({ total: 3, packed: 1, percent: 33 });
    const groups = ops.tripGroups(doc, doc.trips[0]);
    expect(groups.map((g) => g.category.name)).toEqual(['Clothing', 'Toiletries']);
    expect(groups[1].lines.map((l) => l.item.name)).toEqual(['Toothbrush', 'Toothpaste']);
  });

  it('classifies and sorts by date', () => {
    const mk = (name: string, startDate: string, endDate: string) =>
      ({ id: name, name, startDate, endDate, notes: '', createdAt: '', items: [] });
    const today = '2026-06-10';
    const trips = [mk('past', '2026-01-01', '2026-01-05'), mk('later', '2026-09-01', ''), mk('now', '2026-06-09', '2026-06-12'), mk('none', '', '')];
    expect(trips.map((t) => ops.tripPhase(t, today))).toEqual(['past', 'upcoming', 'ongoing', 'undated']);
    const { active, past } = ops.sortTrips(trips, today);
    expect(active.map((t) => t.name)).toEqual(['now', 'later', 'none']);
    expect(past.map((t) => t.name)).toEqual(['past']);
    expect(ops.daysUntil('2026-06-15', today)).toBe(5);
  });
});

describe('backup', () => {
  it('round-trips', () => {
    const s = seeded();
    const t = ops.createTrip(s.doc, { name: 'Rome', startDate: '2026-01-01' }, { categoryIds: [s.clothing] });
    const back = ops.fromBackup(ops.toBackup(t.doc));
    expect(back.trips).toEqual(t.doc.trips);
    expect(back.items).toEqual(t.doc.items);
  });

  it('drops dangling references and rejects junk', () => {
    const s = seeded();
    const t = ops.createTrip(s.doc, { name: 'Rome' }, { categoryIds: [s.clothing] });
    const broken = structuredClone(t.doc);
    broken.items = broken.items.filter((i) => i.id !== s.socks);
    expect(ops.normalizeDoc(broken).trips[0].items).toEqual([]);
    expect(() => ops.fromBackup('{"hello":1}')).toThrow(/Not a QuokkaPack/);
    expect(() => ops.fromBackup('nope')).toThrow(/JSON/);
  });

  it('merges the starter catalog without duplicates', () => {
    const s = seeded();
    const once = ops.addStarterCatalog(s.doc);
    const twice = ops.addStarterCatalog(once);
    expect(twice.items.length).toBe(once.items.length);
    expect(once.onboarded).toBe(true);
  });
});
