// src/app/core/services/categories.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  CategoryReadDto,
  CategoryCreateDto,
  CategoryEditDto,
} from '../../../core/models/api-types'; // adjust path if needed

@Injectable({ providedIn: 'root' })
export class CategoriesService {
  private http = inject(HttpClient);
  private base = '/api/categories';

  /** GET /api/Categories */
  list(): Observable<CategoryReadDto[]> {
    return this.http.get<CategoryReadDto[]>(this.base).pipe(
      map(dtos => (Array.isArray(dtos) ? dtos.map(categoryFromDto) : []))
    );
  }

  /** GET /api/Categories/{id} */
  get(id: number): Observable<CategoryReadDto> {
    return this.http.get<CategoryReadDto>(`${this.base}/${id}`);
  }

  /** POST /api/Categories */
  create(dto: CategoryCreateDto): Observable<CategoryReadDto> {
    return this.http.post<CategoryReadDto>(this.base, dto);
  }

  /** PUT /api/Categories/{id} */
  update(dto: CategoryEditDto): Observable<void> {
    // Controller expects id in route and same id in body
    return this.http.put<void>(`${this.base}/${dto.id}`, dto);
  }

  /** DELETE /api/Categories/{id} */
  remove(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}

/* ---------- module-private mapper (kept for parity with other services) ---------- */
function categoryFromDto(dto: CategoryReadDto): CategoryReadDto {
  return dto;
}
