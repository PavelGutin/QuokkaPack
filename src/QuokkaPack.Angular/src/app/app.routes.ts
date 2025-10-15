import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./home/home').then(m => m.Home) },
  { path: 'login', loadComponent: () => import('./login/login').then(m => m.Login) },
  { path: 'register', loadComponent: () => import('./register/register').then(m => m.Register) },
  { path: 'trips', loadComponent: () => import('./trips/trips-page/trips-page').then(m => m.TripsPage), canActivate: [authGuard] },
  { path: 'trips/create', loadComponent: () => import('./trips/trip-create/trip-create').then(m => m.TripCreate), canActivate: [authGuard] },
  { path: 'trips/:id/pack', loadComponent: () => import('./trips/trip-pack/trip-pack').then(m => m.TripPack), canActivate: [authGuard] },
  { path: 'items', loadComponent: () => import('./items/items').then(m => m.ItemsComponent), canActivate: [authGuard] },
  { path: '**', redirectTo: '' } // 404 fallback
];
