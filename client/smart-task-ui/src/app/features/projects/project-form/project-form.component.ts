import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OperatorFunction } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router, RouterModule } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { ProjectsService } from '../../../core/services/projects.service';
import { NotificationService } from '../../../core/services/notification.service';

// Route: /projects/create or /projects/:id/edit
// Parses the route id in ngOnInit; loads existing project for edit mode.
// canDeactivate prevents accidental navigation with unsaved changes.
@Component({
  selector: 'app-project-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './project-form.component.html',
  styleUrl: './project-form.component.css'
})
export class ProjectFormComponent implements OnInit {
  private untilDestroyed: OperatorFunction<any, any> = takeUntilDestroyed(inject(DestroyRef));
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private projectsService = inject(ProjectsService);
  private notificationService = inject(NotificationService);

  form: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(2000)]],
  });

  loading = signal(false);
  isEdit = false;
  projectId: string | null = null;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'create') {
      this.isEdit = true;
      this.projectId = id;
      this.loadProject(id);
    }
  }

  loadProject(id: string): void {
    this.loading.set(true);
      this.projectsService.get(id).pipe(this.untilDestroyed).subscribe({
      next: (result) => {
        if (result.success && result.data) {
          this.form.patchValue({
            name: result.data.name,
            description: result.data.description || '',
          });
        } else {
          this.notificationService.showError(result.message || 'Failed to load project');
          this.router.navigate(['/projects']);
        }
        this.loading.set(false);
      },
      error: (err: { message?: string }) => {
        this.notificationService.showError(err.message || 'Failed to load project');
        this.router.navigate(['/projects']);
        this.loading.set(false);
      }
    });
  }

  submit(): void {
    if (this.form.invalid || this.loading()) return;

    this.loading.set(true);
    const value = this.form.value;

    if (this.isEdit && this.projectId) {
      this.projectsService.update(this.projectId, value).pipe(this.untilDestroyed).subscribe({
        next: (result) => {
          this.loading.set(false);
          if (result.success && result.data) {
            this.form.markAsPristine();
            this.form.markAsUntouched();
            this.notificationService.showSuccess('Project updated successfully');
            this.router.navigate(['/projects']);
          } else {
            this.notificationService.showError(result.message || 'Update failed');
          }
        },
        error: (err: { message?: string }) => {
          this.loading.set(false);
          this.notificationService.showError(err.message || 'Update failed');
        }
      });
    } else {
      this.projectsService.create({ name: value.name, description: value.description }).pipe(this.untilDestroyed).subscribe({
        next: (result) => {
          this.loading.set(false);
          if (result.success && result.data) {
            this.form.markAsPristine();
            this.form.markAsUntouched();
            this.notificationService.showSuccess('Project created successfully');
            this.router.navigate(['/projects']);
          } else {
            this.notificationService.showError(result.message || 'Creation failed');
          }
        },
        error: (err: { message?: string }) => {
          this.loading.set(false);
          this.notificationService.showError(err.message || 'Creation failed');
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/projects']);
  }

  // Prompt before leaving if the form is dirty
  canDeactivate(): boolean {
    if (this.form.pristine) {
      return true;
    }
    return confirm('You have unsaved changes. Are you sure you want to leave?');
  }
}
