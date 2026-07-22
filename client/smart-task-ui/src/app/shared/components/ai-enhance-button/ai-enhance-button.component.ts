import { Component, inject, input, output, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { AiAvailabilityService } from '../../../core/services/ai.service';
import { TasksService } from '../../../core/services/tasks.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ImproveDescriptionRequest } from '../../../core/models/task';

// Reusable button for AI description improvement.
// Emits the improved description through the `enhanced` output.
// Disabled when the AI service reports it is not configured/enabled.
@Component({
  selector: 'app-ai-enhance-button',
  standalone: true,
  imports: [MatButtonModule, MatTooltipModule, MatProgressSpinnerModule, MatIconModule],
  template: `
    <button
      mat-icon-button
      type="button"
      (click)="enhance()"
      [disabled]="disabled() || loading()"
      [matTooltip]="disabled() ? 'AI description improvement is not configured.' : 'Enhance description with AI'"
      class="ai-btn"
    >
      @if (loading()) {
        <mat-spinner diameter="20"></mat-spinner>
      } @else {
        <mat-icon>auto_awesome</mat-icon>
      }
    </button>
  `,
  styles: [`
    .ai-btn {
      position: absolute;
      right: 8px;
      top: 8px;
    }
  `]
})
export class AiEnhanceButtonComponent {
  private aiService = inject(AiAvailabilityService);
  private tasksService = inject(TasksService);
  private notificationService = inject(NotificationService);

  readonly description = input.required<string>();
  readonly disabled = () => !this.aiService.enabled();
  readonly loading = signal(false);

  enhanced = output<string>();

  // Call backend to improve the description and emit the result
  enhance(): void {
    const text = this.description();
    if (!text) {
      this.notificationService.showError('Please enter a description before enhancing.');
      return;
    }

    this.loading.set(true);
    const request: ImproveDescriptionRequest = { description: text };

    this.tasksService.improveDescription(request).subscribe({
      next: (result) => {
        this.loading.set(false);
        if (result.success && result.data) {
          this.enhanced.emit(result.data.improvedDescription);
        } else {
          this.notificationService.showError(result.message || 'Failed to enhance description');
        }
      },
      error: (err: { message?: string }) => {
        this.loading.set(false);
        this.notificationService.showError(err.message || 'Failed to enhance description');
      }
    });
  }
}
