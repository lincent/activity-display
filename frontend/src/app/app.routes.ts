import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'activities' },
  { path: 'setup', loadComponent: () => import('./features/setup/setup.component').then(m => m.SetupComponent) },
  { path: 'activities', loadComponent: () => import('./features/activities/activities-list.component').then(m => m.ActivitiesListComponent) },
  { path: '**', redirectTo: 'activities' }
];
