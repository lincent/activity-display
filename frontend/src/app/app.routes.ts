import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'activities' },
  { path: 'setup', loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent) },
  { path: 'activities', loadComponent: () => import('./features/activities/activities-list.component').then(m => m.ActivitiesListComponent) },
  { path: '**', redirectTo: 'activities' }
];
