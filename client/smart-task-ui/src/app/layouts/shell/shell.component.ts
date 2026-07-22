import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../core/auth/auth.service';
import { UserRole } from '../../core/models/enums';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterModule, MatSidenavModule, MatToolbarModule, MatListModule, MatButtonModule, MatIconModule],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.css'
})
export class ShellComponent implements OnInit {
  private breakpointObserver = inject(BreakpointObserver);
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);

  readonly isMobile = signal<boolean>(false);
  readonly sidenavOpened = signal<boolean>(false);
  userInitials = '';

  constructor() {}

  ngOnInit(): void {
    const email = this.authService.currentUser()?.email || 'U';
    this.userInitials = email.split('@')[0].slice(0, 2).toUpperCase();

    this.breakpointObserver.observe([Breakpoints.Handset, Breakpoints.Tablet]).subscribe(result => {
      this.isMobile.set(result.matches);
      this.sidenavOpened.set(!result.matches);
    });
  }

  toggleSidenav(): void {
    this.sidenavOpened.update(v => !v);
  }

  logout(): void {
    this.authService.logout();
    this.notificationService.show('Logged out successfully');
  }

  canCreateProject(): boolean {
    return this.authService.hasAnyRole([UserRole.Admin, UserRole.ProjectManager]);
  }

  canCreateTask(): boolean {
    return this.authService.hasAnyRole([UserRole.Admin, UserRole.ProjectManager]);
  }
}
