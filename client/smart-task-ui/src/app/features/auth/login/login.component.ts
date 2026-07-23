import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OperatorFunction } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/auth/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

const SAVED_EMAIL_KEY = 'savedEmail';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit {
  private untilDestroyed: OperatorFunction<any, any> = takeUntilDestroyed(inject(DestroyRef));
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);

  form: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  loading = signal(false);

  ngOnInit(): void {
    const savedEmail = localStorage.getItem(SAVED_EMAIL_KEY);
    if (savedEmail) {
      this.form.patchValue({ email: savedEmail });
    }
  }

  submit(): void {
    if (this.form.invalid || this.loading()) return;

    this.loading.set(true);
    this.authService.login(this.form.value).pipe(this.untilDestroyed).subscribe({
      next: () => {
        const email = this.form.value.email;
        if (email) {
          localStorage.setItem(SAVED_EMAIL_KEY, email);
        }
        this.notificationService.showSuccess('Welcome back!');
        this.router.navigate(['/dashboard']);
      },
      error: (err: Error) => {
        this.notificationService.showError(err.message || 'Invalid email or password.');
        this.loading.set(false);
      }
    });
  }
}
