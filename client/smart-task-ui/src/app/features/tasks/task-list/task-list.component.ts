import { Component } from '@angular/core';
import { RouterLink, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, MatCardModule, RouterLink, RouterModule],
  template: `
    <mat-card>
      <mat-card-content>
        <h2>Tasks</h2>
        <p class="muted">Task list implementation coming soon.</p>
        <a mat-raised-button color="primary" routerLink="/tasks/create">New Task</a>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .muted { color: rgba(0,0,0,0.6); margin-top: 8px; }
  `]
})
export class TaskListComponent {}
