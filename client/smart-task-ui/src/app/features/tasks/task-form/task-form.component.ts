import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [MatCardModule, RouterModule],
  template: `
    <mat-card>
      <mat-card-content>
        <h2>{{ isEdit ? 'Edit' : 'Create' }} Task</h2>
        <p class="muted">Task form implementation coming soon.</p>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .muted { color: rgba(0,0,0,0.6); margin-top: 8px; }
  `]
})
export class TaskFormComponent {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  isEdit = this.route.snapshot.url.some(seg => seg.path === 'edit');

  canDeactivate(): boolean {
    const confirmed = confirm('You have unsaved changes. Are you sure you want to leave?');
    if (!confirmed) {
      return false;
    }
    return true;
  }
}
