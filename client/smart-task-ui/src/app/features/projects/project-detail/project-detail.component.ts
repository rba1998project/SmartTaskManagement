import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [MatCardModule, RouterModule],
  template: `
    <mat-card>
      <mat-card-content>
        <h2>Project {{ id }}</h2>
        <p class="muted">Project detail implementation coming soon.</p>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .muted { color: rgba(0,0,0,0.6); margin-top: 8px; }
  `]
})
export class ProjectDetailComponent {
  private route = inject(ActivatedRoute);
  id = this.route.snapshot.paramMap.get('id') || '';
}
