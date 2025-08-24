//TODO: Factor into separate files. Better yet, get some tooling to generate these automatically

// Keep dates as ISO strings (e.g., "2025-08-23") to match JSON over the wire.
// Expand these later if your Read DTO includes more fields.

export interface TripReadDto {
  id: number;
  destination: string;
  startDate: string; // ISO date string
  endDate: string;   // ISO date string
  categories?: any[];
  // add more fields if your ToReadDto exposes them
}

export interface TripCreateDto {
  destination: string;
  startDate: string; // ISO date string
  endDate: string;   // ISO date string
  categoryIds: number[]; // used by CreateTrip in controller
  categories?: any[];
}

export interface TripEditDto {
  id: number;
  destination: string;
  startDate: string; // ISO date string
  endDate: string;   // ISO date string
}

export interface Trip {
  id: number | string;
  destination: string;
  startDate: string | Date;
  endDate: string | Date;
  categories?: any[];
}

export interface TripItemReadDto {
  id: number;
  itemReadDto: { id: number; name: string; category: { id: number; name: string } };
  quantity: number;
  isPacked: boolean;
  notes?: string | null;
}

export interface TripItemEditDto {
  id: number;
  isPacked: boolean;
}

/** Minimal shapes to match API responses */
export interface CategoryReadDto {
  id: number;
  name: string;
}

export interface ItemReadDto {
  id: number;
  name: string;
  category?: { id: number; name: string };
}

/** For POST /Trips/{id}/TripItems */
export interface TripItemCreateDto {
  itemId: number;
  isPacked: boolean;
}