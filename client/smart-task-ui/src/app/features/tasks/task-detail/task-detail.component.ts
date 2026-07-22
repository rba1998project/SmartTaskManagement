import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-task-detail',
  standalone: true,
  imports: [MatCardModule, RouterModule],
  template: `
    <mat-card>
      <mat-card-content>
        <h2>Task {{ id }}</h2>
        <p class="muted">Task detail implementation coming soon.</p>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .muted { color: rgba(0,0,0,0.6); margin-top: 8px; }
  `]
})
export class TaskDetailComponent {
  private route = inject(ActivatedRoute);
  id = this.route.snapshot.paramMap.get('id') || '';
}
