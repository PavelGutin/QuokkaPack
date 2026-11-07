import { Component, inject, input, signal } from '@angular/core';
import { InstallService } from './install.service';

/**
 * "Install the app": a button that opens the browser's install dialog where the browser allows it, otherwise
 * short instructions. Renders nothing when QuokkaPack already runs installed.
 * - card: for Sync & settings.
 * - link: a small line for the welcome screen; instructions open on tap.
 */
@Component({
  selector: 'qp-install',
  template: `
    @if (!svc.installed()) {
      @if (variant() === 'card') {
        <section class="qp-card" aria-labelledby="install-h">
          <h2 id="install-h">Install the app</h2>
          <p>Opens in its own window from your home screen or app list, works offline, and makes it more likely the browser keeps your data.</p>
          @if (svc.canPrompt()) {
            <div class="grid2"><button type="button" class="qp-btn primary" style="grid-column: span 2" (click)="svc.install()"><i class="bi bi-download" aria-hidden="true"></i>Install QuokkaPack</button></div>
          } @else {
            <p class="qp-install-hint"><i class="bi bi-info-circle me-1" aria-hidden="true"></i>{{ svc.hint }}</p>
          }
        </section>
      } @else {
        <div class="qp-install-link">
          @if (svc.canPrompt()) {
            <button type="button" (click)="svc.install()"><i class="bi bi-download me-1" aria-hidden="true"></i>Install the app</button>
          } @else {
            <button type="button" (click)="open.set(!open())" [attr.aria-expanded]="open()"><i class="bi bi-download me-1" aria-hidden="true"></i>Install the app</button>
            @if (open()) { <p>{{ svc.hint }}</p> }
          }
        </div>
      }
    }
  `,
})
export class InstallPrompt {
  svc = inject(InstallService);
  variant = input<'card' | 'link'>('card');
  open = signal(false);
}
