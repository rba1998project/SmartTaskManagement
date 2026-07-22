import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, RouterModule],
  template: `
    <div class="error-container">
      <mat-card class="error-card">
        <mat-card-content>
          <h1>404</h1>
          <h2>Page Not Found</h2>
          <p>The page you are looking for does not exist.</p>
          <a mat-raised-button color="primary" routerLink="/dashboard">Go to Dashboard</a>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styleUrl: './not-found.component.css'
})
export class NotFoundComponent {}
