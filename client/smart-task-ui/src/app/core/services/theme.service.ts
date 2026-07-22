import { Injectable, signal, effect } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly STORAGE_KEY = 'app-theme';
  private readonly THEME_CLASS = 'dark-theme';

  readonly theme = signal<'light' | 'dark'>('light');

  constructor() {
    const stored = localStorage.getItem(this.STORAGE_KEY);
    if (stored === 'dark') {
      this.theme.set('dark');
    }

    effect(() => {
      const root = document.documentElement;
      if (this.theme() === 'dark') {
        root.classList.add(this.THEME_CLASS);
      } else {
        root.classList.remove(this.THEME_CLASS);
      }
    });
  }

  toggleTheme(): void {
    const next = this.theme() === 'light' ? 'dark' : 'light';
    this.theme.set(next);
    localStorage.setItem(this.STORAGE_KEY, next);
  }
}
