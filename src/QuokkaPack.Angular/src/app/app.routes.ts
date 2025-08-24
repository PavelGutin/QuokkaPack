import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./home/home').then(m => m.Home) }, 
  { path: 'login', loadComponent: () => import('./login/login').then(m => m.Login) },
  { path: 'trips', loadComponent: () => import('./trips/trips-page/trips-page').then(m => m.TripsPage) },
  { path: 'trips/:id/pack', loadComponent: () => import('./trips/trip-pack/trip-pack').then(m => m.TripPack) },
  { path: 'trips/:id/edit', loadComponent: () => import('./trips/trip-edit/trip-edit').then(m => m.TripEdit) },
  { path: 'items', loadComponent: () => import('./items/items').then(m => m.Items) },
  { path: '**', redirectTo: '' } // 404 fallback
];
