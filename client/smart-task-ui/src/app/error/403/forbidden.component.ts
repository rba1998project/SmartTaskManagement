import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-forbidden',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, RouterModule],
  template: `
    <div class="error-container">
      <mat-card class="error-card">
        <mat-card-content>
          <h1>403</h1>
          <h2>Access Denied</h2>
          <p>You do not have permission to access this page.</p>
          <a mat-raised-button color="primary" routerLink="/dashboard">Go to Dashboard</a>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styleUrl: './forbidden.component.css'
})
export class ForbiddenComponent {}
