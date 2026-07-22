import { Component, inject, signal, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
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
import { passwordValidator, confirmPasswordValidator } from '../../../core/validators/password.validator';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);

  form: FormGroup = this.fb.group({
    fullName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(128), passwordValidator()]],
    confirmPassword: ['', [Validators.required, confirmPasswordValidator('password')]],
  });

  loading = signal(false);

  ngOnInit(): void {
    const savedEmail = localStorage.getItem('savedEmail');
    if (savedEmail) {
      this.form.patchValue({ email: savedEmail });
    }
  }

  submit(): void {
    if (this.form.invalid || this.loading()) return;

    const password = this.form.value.password;
    const confirmPassword = this.form.value.confirmPassword;

    if (password !== confirmPassword) {
      this.notificationService.showError('Passwords do not match');
      return;
    }

    this.loading.set(true);
    this.authService.register(this.form.value).pipe(takeUntilDestroyed()).subscribe({
      next: () => {
        this.notificationService.showSuccess('Registration successful! Please sign in.');
        this.router.navigate(['/login']);
      },
      error: (err: Error) => {
        this.notificationService.showError(err.message || 'Registration failed.');
        this.loading.set(false);
      }
    });
  }
}
