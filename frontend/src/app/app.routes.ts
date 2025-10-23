import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'activities' },
  { path: 'login', loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent) },
  { path: 'setup', redirectTo: 'login' }, // Backward compatibility
  { path: 'activities', loadComponent: () => import('./features/activities/activities-list.component').then(m => m.ActivitiesListComponent) },
  { path: '**', redirectTo: 'activities' }
];
