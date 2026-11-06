import { Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DriveSync } from '../sync/drive-sync.service';

/**
 * Backup status. "hero" shows on the Trips header with a label; "bar" is icon-only on other pages and only appears
 * when something needs attention. Tapping syncs, which is a click, so Google's window is allowed to open.
 */
@Component({
  selector: 'qp-sync-chip',
  imports: [RouterLink],
  template: `
    @switch (state()) {
      @case ('off') {
        @if (variant() === 'hero') {
          <a class="qp-sync-chip warn" routerLink="/backup" title="Data is only in this browser. Turn on Google Drive sync in Sync & settings.">
            <i class="bi bi-cloud-slash" aria-hidden="true"></i><span>Not backed up</span>
          </a>
        }
      }
      @case ('synced') {
        @if (variant() === 'hero') {
          <button type="button" class="qp-sync-chip icon" [attr.aria-label]="'Synced ' + when() + '. Sync again'" [title]="'Synced ' + when()" (click)="sync.syncNow()">
            <i class="bi bi-cloud-check" aria-hidden="true"></i>
          </button>
        }
      }
      @case ('check') {
        @if (variant() === 'hero') {
          <button type="button" class="qp-sync-chip" aria-label="Sync with Google Drive" title="Check Google Drive for changes" (click)="sync.syncNow()">
            <i class="bi bi-arrow-repeat" aria-hidden="true"></i><span>Sync</span>
          </button>
        }
      }
      @case ('unsynced') {
        <button type="button" class="qp-sync-chip warn" [class.icon]="variant() === 'bar'" aria-label="Not backed up. Sync now" title="Changes here aren’t in Google Drive yet. Tap to sync." (click)="sync.syncNow()">
          <i class="bi bi-cloud-arrow-up" aria-hidden="true"></i>@if (variant() === 'hero') { <span>Not backed up</span> }
        </button>
      }
      @case ('syncing') {
        <span class="qp-sync-chip" [class.icon]="variant() === 'bar'" role="status" aria-label="Syncing">
          <span class="spinner-border spinner-border-sm" aria-hidden="true"></span>@if (variant() === 'hero') { <span>Syncing</span> }
        </span>
      }
      @case ('error') {
        <button type="button" class="qp-sync-chip warn" [class.icon]="variant() === 'bar'" [attr.aria-label]="(sync.error() || 'Sync failed') + ' Tap to try again'" [title]="sync.error() || 'Sync failed'" (click)="retry()">
          <i class="bi bi-exclamation-triangle" aria-hidden="true"></i>@if (variant() === 'hero') { <span>{{ sync.status() === 'reconnect' ? 'Reconnect' : 'Sync failed' }}</span> }
        </button>
      }
    }
  `,
})
export class SyncChip {
  sync = inject(DriveSync);
  variant = input<'hero' | 'bar'>('hero');
  state = this.sync.indicator;

  when = computed(() => {
    const at = this.sync.lastSyncedAt();
    return at ? new Date(at).toLocaleTimeString(undefined, { hour: 'numeric', minute: '2-digit' }) : '';
  });

  retry() {
    return this.sync.status() === 'reconnect' ? this.sync.connect() : this.sync.syncNow();
  }
}
