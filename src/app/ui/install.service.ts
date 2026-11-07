import { Injectable, signal } from '@angular/core';
import { INSTALL_HINT_TEXT, installHint } from './install-hint';

/** Chromium's install event; not in the DOM typings. */
interface BeforeInstallPromptEvent extends Event {
  prompt(): Promise<void>;
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>;
}

declare global {
  interface Window {
    /** Set by the inline script in index.html if the browser offered installing before the app started. */
    __qpInstallEvent?: BeforeInstallPromptEvent | null;
  }
}

/**
 * Lets people install QuokkaPack from the app. Chromium browsers fire `beforeinstallprompt` when the app can be
 * installed; we keep that event so a button can open the browser's install dialog later. Other browsers get
 * instructions. Nothing shows once the app runs installed.
 */
@Injectable({ providedIn: 'root' })
export class InstallService {
  private deferred: BeforeInstallPromptEvent | null = window.__qpInstallEvent ?? null;

  /** The browser can show its install dialog right now. */
  readonly canPrompt = signal(!!this.deferred);
  /** Running as the installed app, or just installed. */
  readonly installed = signal(isStandalone());
  readonly hint = INSTALL_HINT_TEXT[installHint(navigator.userAgent, navigator.maxTouchPoints)];

  constructor() {
    window.addEventListener('beforeinstallprompt', (e) => {
      this.deferred = e as BeforeInstallPromptEvent;
      this.canPrompt.set(true);
    });
    window.addEventListener('appinstalled', () => {
      this.deferred = null;
      this.canPrompt.set(false);
      this.installed.set(true);
    });
  }

  /** Opens the browser's install dialog. Call from a click. Resolves true if the person installed. */
  async install(): Promise<boolean> {
    const e = this.deferred;
    if (!e) return false;
    // The event can be used once; the browser fires a new one if installing is still possible.
    this.deferred = null;
    this.canPrompt.set(false);
    try {
      await e.prompt();
      const { outcome } = await e.userChoice;
      if (outcome === 'accepted') this.installed.set(true);
      return outcome === 'accepted';
    } catch {
      return false;
    }
  }
}

function isStandalone(): boolean {
  const nav = navigator as Navigator & { standalone?: boolean };
  return (
    nav.standalone === true ||
    (typeof matchMedia === 'function' && (matchMedia('(display-mode: standalone)').matches || matchMedia('(display-mode: window-controls-overlay)').matches))
  );
}
