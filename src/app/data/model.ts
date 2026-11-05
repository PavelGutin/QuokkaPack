/** Everything QuokkaPack stores lives in one JSON document. */

export const SCHEMA_VERSION = 1;

export interface Category {
  id: string;
  name: string;
  archived: boolean;
}

export interface Item {
  id: string;
  name: string;
  categoryId: string;
  archived: boolean;
}

export interface TripItem {
  id: string;
  itemId: string;
  packed: boolean;
  quantity: number;
}

export interface Trip {
  id: string;
  name: string;
  /** ISO date (YYYY-MM-DD) or empty when unknown. */
  startDate: string;
  endDate: string;
  notes: string;
  createdAt: string;
  items: TripItem[];
}

export interface QuokkaDoc {
  version: number;
  categories: Category[];
  items: Item[];
  trips: Trip[];
  /** Set once the first-run starter catalog has been offered. */
  onboarded: boolean;
  updatedAt: string;
}

export function emptyDoc(): QuokkaDoc {
  return { version: SCHEMA_VERSION, categories: [], items: [], trips: [], onboarded: false, updatedAt: '' };
}

export function uid(): string {
  if (typeof crypto !== 'undefined' && 'randomUUID' in crypto) {
    try {
      return crypto.randomUUID();
    } catch {
      /* insecure context – fall through */
    }
  }
  return Date.now().toString(36) + Math.random().toString(36).slice(2, 10);
}
