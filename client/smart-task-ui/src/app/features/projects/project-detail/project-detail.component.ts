import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { ProjectsService } from '../../../core/services/projects.service';
import { ProjectResponse } from '../../../core/models/project';
import { AuthService } from '../../../core/auth/auth.service';
import { UserRole } from '../../../core/models/enums';

// Route: /projects/:id
// Reads the project id from the route snapshot and loads the project.
@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatChipsModule,
  ],
  templateUrl: './project-detail.component.html',
  styleUrl: './project-detail.component.css'
})
export class ProjectDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private projectsService = inject(ProjectsService);
  private authService = inject(AuthService);

  readonly project = signal<ProjectResponse | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  projectId = '';

  canMutate(): boolean {
    return this.authService.hasAnyRole([UserRole.Admin, UserRole.ProjectManager]);
  }

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('id') || '';
    if (this.projectId) {
      this.loadProject();
    } else {
      this.loading.set(false);
      this.error.set('Project not found');
    }
  }

  loadProject(): void {
    this.loading.set(true);
    this.error.set(null);

    this.projectsService.get(this.projectId).subscribe({
      next: (result) => {
        this.loading.set(false);
        if (result.success && result.data) {
          this.project.set(result.data);
        } else {
          this.error.set(result.message || 'Project not found');
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.message || 'Failed to load project');
      }
    });
  }

  // Navigate to the project edit route: /projects/:id/edit
  goToEdit(): void {
    this.router.navigate(['/projects', this.projectId, 'edit']);
  }

  goBack(): void {
    this.router.navigate(['/projects']);
  }
}
