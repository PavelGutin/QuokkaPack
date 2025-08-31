// src/app/core/services/items.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  ItemReadDto,
  ItemCreateDto,
  ItemEditDto,
} from '../../../core/models/api-types'; // adjust path if needed

@Injectable({ providedIn: 'root' })
export class ItemsService {
  private http = inject(HttpClient);
  private base = '/api/items';

  /** GET /api/Items */
  list(): Observable<ItemReadDto[]> {
    return this.http.get<ItemReadDto[]>(this.base).pipe(
      map(dtos => (Array.isArray(dtos) ? dtos.map(itemFromDto) : []))
    );
  }

  /** GET /api/Items/{id} */
  get(id: number): Observable<ItemReadDto> {
    return this.http.get<ItemReadDto>(`${this.base}/${id}`);
  }

  /** POST /api/Items */
  create(dto: ItemCreateDto): Observable<ItemReadDto> {
    return this.http.post<ItemReadDto>(this.base, dto);
  }

  /** PUT /api/Items/{id} */
  update(dto: ItemEditDto): Observable<void> {
    // ItemsController expects id in route and same id in body
    return this.http.put<void>(`${this.base}/${dto.id}`, dto);
  }

  /** DELETE /api/Items/{id} */
  remove(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}

/* ---------- module-private mapper (keep consistent with TripsService) ---------- */
function itemFromDto(dto: ItemReadDto): ItemReadDto {
  return dto;
}
