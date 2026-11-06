import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./pages/trips/trips').then((m) => m.TripsPage), title: 'QuokkaPack' },
  { path: 'trips/new', loadComponent: () => import('./pages/trip-new/trip-new').then((m) => m.TripNewPage), title: 'New trip · QuokkaPack' },
  { path: 'trips/:id', loadComponent: () => import('./pages/trip/trip').then((m) => m.TripPage), title: 'Trip · QuokkaPack' },
  { path: 'catalog', loadComponent: () => import('./pages/catalog/catalog').then((m) => m.CatalogPage), title: 'Catalog · QuokkaPack' },
  { path: 'backup', loadComponent: () => import('./pages/backup/backup').then((m) => m.BackupPage), title: 'Sync & settings · QuokkaPack' },
  { path: '**', redirectTo: '' },
];
