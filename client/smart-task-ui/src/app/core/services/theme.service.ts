import { DOCUMENT } from '@angular/common';
import { computed, inject, Injectable, signal } from '@angular/core';

export type AppTheme = 'classic' | 'field-ledger';

const THEME_STORAGE_KEY = 'smart-task-theme';
const DEFAULT_THEME: AppTheme = 'field-ledger';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  private readonly selectedTheme = signal<AppTheme>(this.readStoredTheme());

  readonly theme = this.selectedTheme.asReadonly();
  readonly isFieldLedger = computed(() => this.selectedTheme() === 'field-ledger');
  readonly toggleLabel = computed(() =>
    this.isFieldLedger() ? 'Switch to Classic theme' : 'Switch to Field Ledger theme'
  );
  readonly toggleIcon = computed(() => this.isFieldLedger() ? 'contrast' : 'palette');

  initialize(): void {
    this.applyTheme(this.selectedTheme());
  }

  toggle(): void {
    this.setTheme(this.isFieldLedger() ? 'classic' : 'field-ledger');
  }

  setTheme(theme: AppTheme): void {
    this.selectedTheme.set(theme);
    this.applyTheme(theme);

    try {
      localStorage.setItem(THEME_STORAGE_KEY, theme);
    } catch {
      // The selected theme still applies for the current session when storage is unavailable.
    }
  }

  private applyTheme(theme: AppTheme): void {
    this.document.documentElement.dataset['theme'] = theme;
    this.document.documentElement.style.colorScheme = 'light';
  }

  private readStoredTheme(): AppTheme {
    try {
      const storedTheme = localStorage.getItem(THEME_STORAGE_KEY);
      return storedTheme === 'classic' || storedTheme === 'field-ledger'
        ? storedTheme
        : DEFAULT_THEME;
    } catch {
      return DEFAULT_THEME;
    }
  }
}
