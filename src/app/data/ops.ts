/**
 * Pure operations on the QuokkaPack document. Every operation takes a document and returns a new one;
 * the input is never mutated, which makes undo and persistence trivial.
 */
import { Category, Item, QuokkaDoc, SCHEMA_VERSION, Trip, TripItem, emptyDoc, uid } from './model';
import { STARTER_CATALOG } from './starter';

type Mutator = (d: QuokkaDoc) => void;

/** Clone, apply mutation, stamp. */
export function apply(doc: QuokkaDoc, fn: Mutator): QuokkaDoc {
  const next = structuredClone(doc);
  fn(next);
  next.updatedAt = new Date().toISOString();
  return next;
}

const norm = (s: string) => s.trim().replace(/\s+/g, ' ');
const same = (a: string, b: string) => norm(a).toLocaleLowerCase() === norm(b).toLocaleLowerCase();

export const byName = <T extends { name: string }>(a: T, b: T) =>
  a.name.localeCompare(b.name, undefined, { sensitivity: 'base', numeric: true });

/* ---------------------------------------------------------------- categories */

export function findCategoryByName(doc: QuokkaDoc, name: string): Category | undefined {
  return doc.categories.find((c) => same(c.name, name));
}

/** Adds a category, or returns the doc unchanged (with the existing id) if one with that name exists. */
export function addCategory(doc: QuokkaDoc, name: string): { doc: QuokkaDoc; id: string } {
  const clean = norm(name);
  if (!clean) throw new Error('Category name is required');
  const existing = findCategoryByName(doc, clean);
  if (existing) {
    if (!existing.archived) return { doc, id: existing.id };
    return { doc: apply(doc, (d) => (d.categories.find((c) => c.id === existing.id)!.archived = false)), id: existing.id };
  }
  const id = uid();
  return { doc: apply(doc, (d) => d.categories.push({ id, name: clean, archived: false })), id };
}

export function renameCategory(doc: QuokkaDoc, id: string, name: string): QuokkaDoc {
  const clean = norm(name);
  if (!clean) throw new Error('Category name is required');
  const clash = findCategoryByName(doc, clean);
  if (clash && clash.id !== id) throw new Error(`A category named “${clash.name}” already exists`);
  return apply(doc, (d) => (d.categories.find((c) => c.id === id)!.name = clean));
}

export function setCategoryArchived(doc: QuokkaDoc, id: string, archived: boolean): QuokkaDoc {
  return apply(doc, (d) => {
    d.categories.find((c) => c.id === id)!.archived = archived;
    if (!archived) d.items.filter((i) => i.categoryId === id).forEach((i) => (i.archived = false));
  });
}

/** Deletes a category, its items, and removes those items from every trip. */
export function deleteCategory(doc: QuokkaDoc, id: string): QuokkaDoc {
  return apply(doc, (d) => {
    const doomed = new Set(d.items.filter((i) => i.categoryId === id).map((i) => i.id));
    d.categories = d.categories.filter((c) => c.id !== id);
    d.items = d.items.filter((i) => !doomed.has(i.id));
    d.trips.forEach((t) => (t.items = t.items.filter((ti) => !doomed.has(ti.itemId))));
  });
}

/* --------------------------------------------------------------------- items */

export function addItem(doc: QuokkaDoc, categoryId: string, name: string): { doc: QuokkaDoc; id: string } {
  const clean = norm(name);
  if (!clean) throw new Error('Item name is required');
  if (!doc.categories.some((c) => c.id === categoryId)) throw new Error('Unknown category');
  const existing = doc.items.find((i) => i.categoryId === categoryId && same(i.name, clean));
  if (existing) {
    if (!existing.archived) return { doc, id: existing.id };
    return { doc: apply(doc, (d) => (d.items.find((i) => i.id === existing.id)!.archived = false)), id: existing.id };
  }
  const id = uid();
  return { doc: apply(doc, (d) => d.items.push({ id, name: clean, categoryId, archived: false })), id };
}

export function updateItem(doc: QuokkaDoc, id: string, patch: Partial<Pick<Item, 'name' | 'categoryId'>>): QuokkaDoc {
  const name = patch.name !== undefined ? norm(patch.name) : undefined;
  if (name === '') throw new Error('Item name is required');
  return apply(doc, (d) => {
    const item = d.items.find((i) => i.id === id)!;
    if (name !== undefined) item.name = name;
    if (patch.categoryId) item.categoryId = patch.categoryId;
  });
}

export function setItemArchived(doc: QuokkaDoc, id: string, archived: boolean): QuokkaDoc {
  return apply(doc, (d) => {
    const item = d.items.find((i) => i.id === id)!;
    item.archived = archived;
    if (!archived) d.categories.find((c) => c.id === item.categoryId)!.archived = false;
  });
}

export function deleteItem(doc: QuokkaDoc, id: string): QuokkaDoc {
  return apply(doc, (d) => {
    d.items = d.items.filter((i) => i.id !== id);
    d.trips.forEach((t) => (t.items = t.items.filter((ti) => ti.itemId !== id)));
  });
}

/* --------------------------------------------------------------------- trips */

export interface TripInput {
  name: string;
  startDate?: string;
  endDate?: string;
  notes?: string;
}

function cleanTripInput(input: TripInput) {
  const name = norm(input.name);
  if (!name) throw new Error('Trip name is required');
  const startDate = input.startDate ?? '';
  let endDate = input.endDate ?? '';
  if (startDate && endDate && endDate < startDate) throw new Error('End date is before start date');
  return { name, startDate, endDate, notes: (input.notes ?? '').trim() };
}

export interface CreateTripOptions {
  /** Include every active item of these categories. */
  categoryIds?: string[];
  /** Copy the item list (unpacked) of an existing trip. */
  copyFromTripId?: string;
}

export function createTrip(doc: QuokkaDoc, input: TripInput, opts: CreateTripOptions = {}): { doc: QuokkaDoc; id: string } {
  const fields = cleanTripInput(input);
  const id = uid();
  const next = apply(doc, (d) => {
    const items: TripItem[] = [];
    const seen = new Set<string>();
    const push = (itemId: string, quantity = 1) => {
      if (seen.has(itemId)) return;
      seen.add(itemId);
      items.push({ id: uid(), itemId, packed: false, quantity });
    };
    if (opts.copyFromTripId) {
      d.trips.find((t) => t.id === opts.copyFromTripId)?.items.forEach((ti) => push(ti.itemId, ti.quantity));
    }
    const cats = new Set(opts.categoryIds ?? []);
    d.items.filter((i) => !i.archived && cats.has(i.categoryId)).forEach((i) => push(i.id));
    d.trips.push({ id, ...fields, createdAt: new Date().toISOString(), items });
  });
  return { doc: next, id };
}

export function updateTrip(doc: QuokkaDoc, id: string, input: TripInput): QuokkaDoc {
  const fields = cleanTripInput(input);
  return apply(doc, (d) => Object.assign(d.trips.find((t) => t.id === id)!, fields));
}

export function duplicateTrip(doc: QuokkaDoc, id: string): { doc: QuokkaDoc; id: string } {
  const src = doc.trips.find((t) => t.id === id);
  if (!src) throw new Error('Unknown trip');
  return createTrip(doc, { name: `${src.name} (copy)`, notes: src.notes }, { copyFromTripId: id });
}

export function deleteTrip(doc: QuokkaDoc, id: string): QuokkaDoc {
  return apply(doc, (d) => (d.trips = d.trips.filter((t) => t.id !== id)));
}

function onTrip(doc: QuokkaDoc, tripId: string, fn: (t: Trip, d: QuokkaDoc) => void): QuokkaDoc {
  return apply(doc, (d) => fn(d.trips.find((t) => t.id === tripId)!, d));
}

export function addItemsToTrip(doc: QuokkaDoc, tripId: string, itemIds: string[]): QuokkaDoc {
  return onTrip(doc, tripId, (t) => {
    const present = new Set(t.items.map((ti) => ti.itemId));
    for (const itemId of itemIds) {
      if (present.has(itemId)) continue;
      present.add(itemId);
      t.items.push({ id: uid(), itemId, packed: false, quantity: 1 });
    }
  });
}

export function removeItemsFromTrip(doc: QuokkaDoc, tripId: string, itemIds: string[]): QuokkaDoc {
  const drop = new Set(itemIds);
  return onTrip(doc, tripId, (t) => (t.items = t.items.filter((ti) => !drop.has(ti.itemId))));
}

/** Creates a catalog item (or reuses a same-named one) and puts it on the trip. */
export function quickAddToTrip(doc: QuokkaDoc, tripId: string, categoryId: string, name: string): QuokkaDoc {
  const { doc: withItem, id } = addItem(doc, categoryId, name);
  return addItemsToTrip(withItem, tripId, [id]);
}

export function setPacked(doc: QuokkaDoc, tripId: string, tripItemIds: string[], packed: boolean): QuokkaDoc {
  const ids = new Set(tripItemIds);
  return onTrip(doc, tripId, (t) => t.items.forEach((ti) => ids.has(ti.id) && (ti.packed = packed)));
}

export function setQuantity(doc: QuokkaDoc, tripId: string, tripItemId: string, quantity: number): QuokkaDoc {
  const q = Math.max(1, Math.min(99, Math.round(quantity) || 1));
  return onTrip(doc, tripId, (t) => (t.items.find((ti) => ti.id === tripItemId)!.quantity = q));
}

export function resetPacking(doc: QuokkaDoc, tripId: string): QuokkaDoc {
  return onTrip(doc, tripId, (t) => t.items.forEach((ti) => (ti.packed = false)));
}

/* ----------------------------------------------------------------- views */

export interface TripLine {
  tripItem: TripItem;
  item: Item;
}

export interface TripGroup {
  category: Category;
  lines: TripLine[];
  packed: number;
}

export function tripGroups(doc: QuokkaDoc, trip: Trip): TripGroup[] {
  const items = new Map(doc.items.map((i) => [i.id, i]));
  const cats = new Map(doc.categories.map((c) => [c.id, c]));
  const groups = new Map<string, TripGroup>();
  for (const tripItem of trip.items) {
    const item = items.get(tripItem.itemId);
    const category = item && cats.get(item.categoryId);
    if (!item || !category) continue;
    let g = groups.get(category.id);
    if (!g) groups.set(category.id, (g = { category, lines: [], packed: 0 }));
    g.lines.push({ tripItem, item });
    if (tripItem.packed) g.packed++;
  }
  const out = [...groups.values()].sort((a, b) => byName(a.category, b.category));
  out.forEach((g) => g.lines.sort((a, b) => byName(a.item, b.item)));
  return out;
}

export interface TripStats {
  total: number;
  packed: number;
  percent: number;
}

export function tripStats(trip: Trip): TripStats {
  const total = trip.items.length;
  const packed = trip.items.filter((i) => i.packed).length;
  return { total, packed, percent: total ? Math.round((packed / total) * 100) : 0 };
}

export type TripPhase = 'upcoming' | 'ongoing' | 'past' | 'undated';

export function todayIso(now = new Date()): string {
  const p = (n: number) => String(n).padStart(2, '0');
  return `${now.getFullYear()}-${p(now.getMonth() + 1)}-${p(now.getDate())}`;
}

export function tripPhase(trip: Trip, today = todayIso()): TripPhase {
  if (!trip.startDate && !trip.endDate) return 'undated';
  const start = trip.startDate || trip.endDate;
  const end = trip.endDate || trip.startDate;
  if (today < start) return 'upcoming';
  if (today > end) return 'past';
  return 'ongoing';
}

/** Whole days from `today` to `date` (both ISO). */
export function daysUntil(date: string, today = todayIso()): number {
  const ms = Date.parse(date + 'T00:00:00Z') - Date.parse(today + 'T00:00:00Z');
  return Math.round(ms / 86_400_000);
}

/** Upcoming/ongoing first (soonest first), then undated (newest first), then past (most recent first). */
export function sortTrips(trips: Trip[], today = todayIso()): { active: Trip[]; past: Trip[] } {
  const active = trips.filter((t) => tripPhase(t, today) !== 'past');
  const past = trips.filter((t) => tripPhase(t, today) === 'past');
  active.sort((a, b) => {
    const ka = a.startDate || a.endDate || '9999';
    const kb = b.startDate || b.endDate || '9999';
    return ka === kb ? b.createdAt.localeCompare(a.createdAt) : ka.localeCompare(kb);
  });
  past.sort((a, b) => (b.endDate || b.startDate).localeCompare(a.endDate || a.startDate));
  return { active, past };
}

/* ------------------------------------------------------------ starter data */

/** Merges the starter catalog into the doc (existing names are kept, not duplicated). */
export function addStarterCatalog(doc: QuokkaDoc): QuokkaDoc {
  let next = doc;
  for (const [catName, itemNames] of Object.entries(STARTER_CATALOG)) {
    const { doc: d1, id } = addCategory(next, catName);
    next = d1;
    for (const n of itemNames) next = addItem(next, id, n).doc;
  }
  return apply(next, (d) => (d.onboarded = true));
}

/* ---------------------------------------------------------- import/export */

export const BACKUP_KIND = 'quokkapack-backup';

export function toBackup(doc: QuokkaDoc): string {
  return JSON.stringify({ kind: BACKUP_KIND, exportedAt: new Date().toISOString(), data: doc }, null, 2);
}

/** Parses a backup (or a bare document) and returns a validated, normalised document. Throws on bad input. */
export function fromBackup(text: string): QuokkaDoc {
  let raw: unknown;
  try {
    raw = JSON.parse(text);
  } catch {
    throw new Error('That file is not valid JSON');
  }
  const obj = raw as Record<string, unknown>;
  const data = (obj && obj['kind'] === BACKUP_KIND ? obj['data'] : obj) as Record<string, unknown>;
  return normalizeDoc(data);
}

/** Validates and repairs a document read from storage or a backup. */
export function normalizeDoc(data: unknown): QuokkaDoc {
  if (!data || typeof data !== 'object') throw new Error('Not a QuokkaPack backup');
  const d = data as Record<string, unknown>;
  if (!Array.isArray(d['categories']) || !Array.isArray(d['items']) || !Array.isArray(d['trips'])) {
    throw new Error('Not a QuokkaPack backup');
  }
  if (typeof d['version'] === 'number' && d['version'] > SCHEMA_VERSION) {
    throw new Error('This backup was made by a newer version of QuokkaPack');
  }
  const str = (v: unknown) => (typeof v === 'string' ? v : '');
  const out = emptyDoc();
  out.onboarded = d['onboarded'] !== false;
  out.updatedAt = str(d['updatedAt']);

  for (const c of d['categories'] as Record<string, unknown>[]) {
    if (!c || !str(c['id']) || !str(c['name']).trim()) continue;
    out.categories.push({ id: str(c['id']), name: str(c['name']).trim(), archived: c['archived'] === true });
  }
  const catIds = new Set(out.categories.map((c) => c.id));
  for (const i of d['items'] as Record<string, unknown>[]) {
    if (!i || !str(i['id']) || !str(i['name']).trim() || !catIds.has(str(i['categoryId']))) continue;
    out.items.push({ id: str(i['id']), name: str(i['name']).trim(), categoryId: str(i['categoryId']), archived: i['archived'] === true });
  }
  const itemIds = new Set(out.items.map((i) => i.id));
  for (const t of d['trips'] as Record<string, unknown>[]) {
    if (!t || !str(t['id'])) continue;
    const seen = new Set<string>();
    const items: TripItem[] = [];
    for (const ti of (Array.isArray(t['items']) ? t['items'] : []) as Record<string, unknown>[]) {
      const itemId = str(ti?.['itemId']);
      if (!itemIds.has(itemId) || seen.has(itemId)) continue;
      seen.add(itemId);
      const q = Number(ti['quantity']);
      items.push({ id: str(ti['id']) || uid(), itemId, packed: ti['packed'] === true, quantity: q >= 1 ? Math.min(99, Math.round(q)) : 1 });
    }
    out.trips.push({
      id: str(t['id']),
      name: str(t['name']).trim() || 'Untitled trip',
      startDate: /^\d{4}-\d{2}-\d{2}$/.test(str(t['startDate'])) ? str(t['startDate']) : '',
      endDate: /^\d{4}-\d{2}-\d{2}$/.test(str(t['endDate'])) ? str(t['endDate']) : '',
      notes: str(t['notes']),
      createdAt: str(t['createdAt']) || new Date().toISOString(),
      items,
    });
  }
  return out;
}
