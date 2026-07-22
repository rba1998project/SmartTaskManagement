import { Component } from '@angular/core';
import { RouterLink, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatTableModule, RouterLink, RouterModule],
  template: `
    <mat-card>
      <mat-card-content>
        <h2>Projects</h2>
        <p class="muted">Project list implementation coming soon.</p>
        <a mat-raised-button color="primary" routerLink="/projects/create">New Project</a>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .muted { color: rgba(0,0,0,0.6); margin-top: 8px; }
  `]
})
export class ProjectListComponent {}
