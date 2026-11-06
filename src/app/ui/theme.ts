import { Injectable, signal } from '@angular/core';
import { THEME_COLOR, THEME_KEY, ThemePref, Theme, parsePref, resolveTheme } from './theme-pref';

export type { ThemePref, Theme } from './theme-pref';

/**
 * Light or dark, following the system unless the person picks one. index.html applies the same rule before
 * Angular starts, so there is no flash of the wrong theme.
 */
@Injectable({ providedIn: 'root' })
export class ThemeService {
  private media = typeof matchMedia === 'function' ? matchMedia('(prefers-color-scheme: dark)') : null;
  readonly pref = signal<ThemePref>(readPref());
  readonly theme = signal<Theme>('light');

  constructor() {
    this.media?.addEventListener('change', () => this.apply());
    this.apply();
  }

  set(pref: ThemePref): void {
    this.pref.set(pref);
    try {
      if (pref === 'system') localStorage.removeItem(THEME_KEY);
      else localStorage.setItem(THEME_KEY, pref);
    } catch {
      /* storage blocked: applies for this visit only */
    }
    this.apply();
  }

  private apply(): void {
    const theme = resolveTheme(this.pref(), !!this.media?.matches);
    this.theme.set(theme);
    document.documentElement.setAttribute('data-bs-theme', theme);
    document.querySelector('meta[name="theme-color"]')?.setAttribute('content', THEME_COLOR[theme]);
  }
}

function readPref(): ThemePref {
  try {
    return parsePref(localStorage.getItem(THEME_KEY));
  } catch {
    return 'system';
  }
}
