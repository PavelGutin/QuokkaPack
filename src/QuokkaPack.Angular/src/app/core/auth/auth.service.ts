// src/app/core/auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, tap } from 'rxjs';
import { jwtDecode } from 'jwt-decode';

type LoginDto = { Email: string; Password: string };
type RegisterDto = { Email: string; Password: string; ConfirmPassword: string };
type LoginResponse = { token: string };
type JwtPayload = { exp?: number; email?: string; unique_name?: string };

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly key = 'qp_token';

  /** Emits true only when a non-expired token exists */
  authChanged = new BehaviorSubject<boolean>(this.hasValidToken());

  constructor(private http: HttpClient) {}

  // --- Public API -----------------------------------------------------------

  login(body: LoginDto) {
    return this.http.post<LoginResponse>('/api/auth/login', body).pipe(
      tap(res => {
        localStorage.setItem(this.key, res.token);
        this.authChanged.next(true);
      })
    );
  }

  register(body: RegisterDto) {
    return this.http.post<LoginResponse>('/api/auth/register', body).pipe(
      tap(res => {
        localStorage.setItem(this.key, res.token);
        this.authChanged.next(true);
      })
    );
  }

  logout() {
    localStorage.removeItem(this.key);
    this.authChanged.next(false);
  }

  /** Raw token from storage (or null) */
  get token(): string | null {
    return localStorage.getItem(this.key);
  }

  /** Friendly name from token (email or unique_name) */
  get userName(): string {
    const p = this.payload();
    return (p?.email || p?.unique_name || '').trim();
  }

  /** True iff token exists and exp is in the future */
  hasValidToken(): boolean {
    const p = this.payload();
    if (!p?.exp) return false;
    return (Date.now() / 1000) < p.exp;
  }

  // --- Helpers --------------------------------------------------------------

  private payload(): JwtPayload | null {
    const t = this.token;
    if (!t) return null;
    try {
      return jwtDecode<JwtPayload>(t);
    } catch {
      return null;
    }
  }
}
