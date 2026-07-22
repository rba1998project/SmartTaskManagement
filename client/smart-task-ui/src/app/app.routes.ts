import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { unsavedChangesGuard } from './core/guards/unsaved-changes.guard';
import { UserRole } from './core/models/enums';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent) },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layouts/shell/shell.component').then(m => m.ShellComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      {
        path: 'projects',
        children: [
          { path: '', loadComponent: () => import('./features/projects/project-list/project-list.component').then(m => m.ProjectListComponent) },
          {
            path: 'create',
            canActivate: [roleGuard],
            canDeactivate: [unsavedChangesGuard],
            data: { roles: [UserRole.Admin, UserRole.ProjectManager] },
            loadComponent: () => import('./features/projects/project-form/project-form.component').then(m => m.ProjectFormComponent)
          },
          { path: ':id', loadComponent: () => import('./features/projects/project-detail/project-detail.component').then(m => m.ProjectDetailComponent) },
          {
            path: ':id/edit',
            canActivate: [roleGuard],
            canDeactivate: [unsavedChangesGuard],
            data: { roles: [UserRole.Admin, UserRole.ProjectManager] },
            loadComponent: () => import('./features/projects/project-form/project-form.component').then(m => m.ProjectFormComponent)
          },
        ]
      },
      {
        path: 'tasks',
        children: [
          { path: '', loadComponent: () => import('./features/tasks/task-list/task-list.component').then(m => m.TaskListComponent) },
          {
            path: 'create',
            canActivate: [roleGuard],
            canDeactivate: [unsavedChangesGuard],
            data: { roles: [UserRole.Admin, UserRole.ProjectManager] },
            loadComponent: () => import('./features/tasks/task-form/task-form.component').then(m => m.TaskFormComponent)
          },
          { path: ':id', loadComponent: () => import('./features/tasks/task-detail/task-detail.component').then(m => m.TaskDetailComponent) },
          {
            path: ':id/edit',
            canActivate: [roleGuard],
            canDeactivate: [unsavedChangesGuard],
            data: { roles: [UserRole.Admin, UserRole.ProjectManager] },
            loadComponent: () => import('./features/tasks/task-form/task-form.component').then(m => m.TaskFormComponent)
          },
        ]
      },
      { path: 'account', loadComponent: () => import('./features/account/profile/profile.component').then(m => m.ProfileComponent) },
      { path: '403', loadComponent: () => import('./error/403/forbidden.component').then(m => m.ForbiddenComponent) },
      { path: '404', loadComponent: () => import('./error/404/not-found.component').then(m => m.NotFoundComponent) },
      { path: '**', redirectTo: '404' },
    ]
  },
  { path: '**', redirectTo: '404' },
];
