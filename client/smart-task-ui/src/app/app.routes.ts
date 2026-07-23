import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { unsavedChangesGuard } from './core/guards/unsaved-changes.guard';
import { UserRole } from './core/models/enums';

// Top-level route configuration.
// Public auth routes (login/register) are siblings of the protected shell.
// The shell acts as a layout wrapper for all authenticated feature routes.
export const routes: Routes = [
  { path: 'login', data: { title: 'Sign In' }, loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', data: { title: 'Create Account' }, loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent) },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layouts/shell/shell.component').then(m => m.ShellComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', data: { title: 'Dashboard' }, loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      {
        path: 'projects',
        children: [
          { path: '', data: { title: 'Projects' }, loadComponent: () => import('./features/projects/project-list/project-list.component').then(m => m.ProjectListComponent) },
          {
            path: 'create',
            canActivate: [roleGuard],
            canDeactivate: [unsavedChangesGuard],
            data: { title: 'Create Project', roles: [UserRole.Admin, UserRole.ProjectManager] },
            loadComponent: () => import('./features/projects/project-form/project-form.component').then(m => m.ProjectFormComponent)
          },
          { path: ':id', data: { title: 'Project Details' }, loadComponent: () => import('./features/projects/project-detail/project-detail.component').then(m => m.ProjectDetailComponent) },
          {
            path: ':id/edit',
            canActivate: [roleGuard],
            canDeactivate: [unsavedChangesGuard],
            data: { title: 'Edit Project', roles: [UserRole.Admin, UserRole.ProjectManager] },
            loadComponent: () => import('./features/projects/project-form/project-form.component').then(m => m.ProjectFormComponent)
          },
        ]
      },
      {
        path: 'tasks',
        children: [
          { path: '', data: { title: 'Tasks' }, loadComponent: () => import('./features/tasks/task-list/task-list.component').then(m => m.TaskListComponent) },
          {
            path: 'create',
            canActivate: [roleGuard],
            canDeactivate: [unsavedChangesGuard],
            data: { title: 'Create Task', roles: [UserRole.Admin, UserRole.ProjectManager] },
            loadComponent: () => import('./features/tasks/task-form/task-form.component').then(m => m.TaskFormComponent)
          },
          { path: ':id', data: { title: 'Task Details' }, loadComponent: () => import('./features/tasks/task-detail/task-detail.component').then(m => m.TaskDetailComponent) },
          {
            path: ':id/edit',
            canActivate: [roleGuard],
            canDeactivate: [unsavedChangesGuard],
            data: { title: 'Edit Task', roles: [UserRole.Admin, UserRole.ProjectManager] },
            loadComponent: () => import('./features/tasks/task-form/task-form.component').then(m => m.TaskFormComponent)
          },
        ]
      },
      { path: 'account', data: { title: 'Account' }, loadComponent: () => import('./features/account/profile/profile.component').then(m => m.ProfileComponent) },
      { path: 'users', canActivate: [roleGuard], data: { title: 'User Management', roles: [UserRole.Admin] }, loadComponent: () => import('./features/user-management/user-management.component').then(m => m.UserManagementComponent) },
      { path: '403', data: { title: 'Forbidden' }, loadComponent: () => import('./error/403/forbidden.component').then(m => m.ForbiddenComponent) },
      { path: '404', data: { title: 'Not Found' }, loadComponent: () => import('./error/404/not-found.component').then(m => m.NotFoundComponent) },
      { path: '**', redirectTo: '404' },
    ]
  },
  { path: '**', redirectTo: '404' },
];
