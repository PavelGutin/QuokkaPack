// src/app/core/services/trips.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import {
  Trip,              // domain model used by components
  TripReadDto,
  TripCreateDto,
  TripEditDto, 
  TripItemReadDto, 
  TripItemEditDto
} from '../../models/trip';

@Injectable({ providedIn: 'root' })
export class TripsService {
  private http = inject(HttpClient);
  private base = '/api/trips';

  // GET api/Trips
  list(): Observable<Trip[]> {
    return this.http.get<TripReadDto[]>(this.base).pipe(
      map(dtos => (Array.isArray(dtos) ? dtos.map(tripFromDto) : []))
    );
  }

  // GET api/Trips/{id}
  get(id: number): Observable<Trip> {
    return this.http.get<TripReadDto>(`${this.base}/${id}`).pipe(
      map(tripFromDto)
    );
  }

  // POST api/Trips
  // (request stays a DTO; response mapped to domain)
  create(dto: TripCreateDto): Observable<Trip> {
    return this.http.post<TripReadDto>(this.base, dto).pipe(
      map(tripFromDto)
    );
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

}

/* ---------- module-private mapper (not exported) ---------- */

function tripFromDto(dto: TripReadDto): Trip {
  return {
    id: dto.id,
    destination: dto.destination,
    startDate: new Date(dto.startDate),
    endDate: new Date(dto.endDate),
    categories: dto.categories ?? [], 
  };
}
