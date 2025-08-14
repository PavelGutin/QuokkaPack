// src/app/app.routes.ts
import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./home/home').then(m => m.Home) },   // <- default
  { path: 'login', loadComponent: () => import('./login/login').then(m => m.Login) },
  { path: 'trips', loadComponent: () => import('./trips/trips').then(m => m.Trips) },  
  { path: 'items', loadComponent: () => import('./items/items').then(m => m.Items) },
  { path: '', pathMatch: 'full', redirectTo: 'login' }
];
