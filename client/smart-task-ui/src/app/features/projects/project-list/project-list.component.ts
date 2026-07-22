import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { ProjectsService } from '../../../core/services/projects.service';
import { ProjectResponse, ProjectQueryRequest } from '../../../core/models/project';
import { AuthService } from '../../../core/auth/auth.service';
import { UserRole } from '../../../core/models/enums';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { PagedResult } from '../../../core/models/enums';

// Route: /projects
// Loads paginated, sortable, and searchable project list.
@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatInputModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    RouterModule,
  ],
  templateUrl: './project-list.component.html',
  styleUrl: './project-list.component.css'
})
export class ProjectListComponent implements OnInit {
  private projectsService = inject(ProjectsService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private dialog = inject(MatDialog);

  readonly projects = signal<ProjectResponse[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly search = signal('');
  readonly sortField = signal<ProjectQueryRequest['sortField']>('CreatedAt');
  readonly sortDirection = signal<'Asc' | 'Desc'>('Desc');
  readonly pageNumber = signal(1);
  readonly pageSize = signal(10);
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);

  displayedColumns: string[] = ['name', 'description', 'createdAt', 'actions'];

  canMutate(): boolean {
    return this.authService.hasAnyRole([UserRole.Admin, UserRole.ProjectManager]);
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    const params: ProjectQueryRequest = {
      search: this.search() || undefined,
      sortField: this.sortField(),
      sortDirection: this.sortDirection(),
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
    };

    this.projectsService.list(params).subscribe({
      next: (result) => {
        if (result.success && result.data) {
          this.projects.set(result.data.items);
          this.totalCount.set(result.data.totalCount);
          this.totalPages.set(result.data.totalPages);
        } else {
          this.error.set(result.message || 'Failed to load projects');
        }
        this.loading.set(false);
      },
      error: (err: { message?: string }) => {
        this.error.set(err.message || 'Network error');
        this.loading.set(false);
      }
    });
  }

  onSearch(value: string): void {
    this.search.set(value);
    this.pageNumber.set(1);
    this.load();
  }

  onSort(sort: { active: string; direction: string }): void {
    if (!sort.active || !sort.direction) return;
    this.sortField.set(sort.active as ProjectQueryRequest['sortField']);
    this.sortDirection.set(sort.direction as 'Asc' | 'Desc');
    this.pageNumber.set(1);
    this.load();
  }

  onPageChange(event: { pageIndex: number; pageSize: number }): void {
    this.pageNumber.set(event.pageIndex + 1);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  goToCreate(): void {
    this.router.navigate(['/projects/create']);
  }

  goToEdit(id: string): void {
    this.router.navigate(['/projects', id, 'edit']);
  }

  goToDetail(id: string): void {
    this.router.navigate(['/projects', id]);
  }

  // Delete project via confirmation dialog
  deleteProject(project: ProjectResponse): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete Project',
        message: `Are you sure you want to delete "${project.name}"?`,
      },
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.projectsService.delete(project.id).subscribe({
          next: () => {
            this.load();
          },
          error: (err: { message?: string }) => {
            this.error.set(err.message || 'Failed to delete project');
          }
        });
      }
    });
  }
}
