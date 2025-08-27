// src/app/core/services/trips.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import {
  TripCreateDto,
  TripEditDto, 
  TripItemReadDto, 
  TripItemEditDto, 
  ItemReadDto, 
  CategoryReadDto, 
  TripItemCreateDto,
  TripSummaryReadDto,
  TripDetailsReadDto
} from '../../../core/models/api-types';

@Injectable({ providedIn: 'root' })
export class TripsService {
  private http = inject(HttpClient);
  private base = '/api/trips';

  // GET api/Trips
  list(): Observable<TripSummaryReadDto[]> {
    return this.http.get<TripSummaryReadDto[]>(this.base).pipe(
      map(dtos => (Array.isArray(dtos) ? dtos.map(tripFromDto) : []))
    );
  }

  // GET api/Trips/{id}
  get(id: number): Observable<TripDetailsReadDto> {
    return this.http.get<TripDetailsReadDto>(`${this.base}/${id}`);
  }

  // POST api/Trips
  // (request stays a DTO; response mapped to domain)
  create(dto: TripCreateDto): Observable<TripCreateDto> {
    return this.http.post<TripCreateDto>(this.base, dto);
  }

  // PUT api/Trips/{id}
  // If your API returns the updated entity, switch void->TripReadDto and map it.
  update(dto: TripEditDto): Observable<void> {
    return this.http.put<void>(`${this.base}/${dto.id}`, dto);
  }

  // DELETE api/Trips/{id}
  remove(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  getItems(tripId: number): Observable<TripItemReadDto[]> {
    return this.http.get<TripItemReadDto[]>(`${this.base}/${tripId}/TripItems`);
  }

  /** PUT /api/Trips/{id}/TripItems/batch  (payload: TripItemEditDto[]) */
  updatePackedStatus(tripId: number, items: TripItemEditDto[]): Observable<void> {
    return this.http.put<void>(`${this.base}/${tripId}/TripItems/batch`, items);
  }

  setTripItemPacked(loadedForId: number, tripItemId: number, checked: boolean){
    return this.http.put<void>(`${this.base}/${tripItemId}/TripItems/batch`, '');
  }

// --- New methods ---
listAllItems() {
  return this.http.get<ItemReadDto[]>('/api/items');
}

listAllCategories() {
  return this.http.get<CategoryReadDto[]>('/api/categories');
}

addTripItem(tripId: number, dto: TripItemCreateDto) {
  return this.http.post<TripItemReadDto>(`${this.base}/${tripId}/TripItems`, dto);
}

deleteTripItem(tripId: number, tripItemId: number) {
  return this.http.delete<void>(`${this.base}/${tripId}/TripItems/${tripItemId}`);
}

addCategoryToTrip(tripId: number, categoryId: number) {
  // Your Razor page posts the plain categoryId; mirror that
  return this.http.post<void>(`${this.base}/${tripId}/Categories`, categoryId);
}

deleteCategoryFromTrip(tripId: number, categoryId: number) {
  return this.http.delete<void>(`${this.base}/${tripId}/Categories/${categoryId}`);
}  

}

/* ---------- module-private mapper (not exported) ---------- */
//TODO: Remove this. Keeping for now to get out of refactoring hell 
function tripFromDto(dto: TripSummaryReadDto): TripSummaryReadDto {
  return dto;
  // return {
  //   id: dto.id,
  //   destination: dto.destination,
  //   startDate: new Date(dto.startDate),
  //   endDate: new Date(dto.endDate),
  //   //categories: dto.categories ?? [], 
  // };
}
