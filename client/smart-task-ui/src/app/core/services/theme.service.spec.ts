import { TestBed } from '@angular/core/testing';

import { ThemeService } from './theme.service';

describe('ThemeService', () => {
  const storageKey = 'smart-task-theme';

  beforeEach(() => {
    localStorage.removeItem(storageKey);
    document.documentElement.removeAttribute('data-theme');
    TestBed.configureTestingModule({});
  });

  afterEach(() => {
    localStorage.removeItem(storageKey);
    document.documentElement.removeAttribute('data-theme');
  });

  it('initializes the Field Ledger theme by default', () => {
    const service = TestBed.inject(ThemeService);

    service.initialize();

    expect(service.theme()).toBe('field-ledger');
    expect(service.toggleLabel()).toBe('Switch to Classic theme');
    expect(document.documentElement.dataset['theme']).toBe('field-ledger');
  });

  it('toggles to Classic and persists the selection', () => {
    const service = TestBed.inject(ThemeService);

    service.toggle();

    expect(service.theme()).toBe('classic');
    expect(service.toggleLabel()).toBe('Switch to Field Ledger theme');
    expect(document.documentElement.dataset['theme']).toBe('classic');
    expect(localStorage.getItem(storageKey)).toBe('classic');
  });
});
