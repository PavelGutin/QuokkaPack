import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./home/home').then(m => m.Home) }, // default
  { path: 'login', loadComponent: () => import('./login/login').then(m => m.Login) },
  { path: 'trips', loadComponent: () => import('./trips/trips-page/trips-page').then(m => m.TripsPage) },
  { path: 'items', loadComponent: () => import('./items/items').then(m => m.Items) },
  { path: '**', redirectTo: '' } // 404 fallback
];
