import { Injectable, inject, signal } from '@angular/core';
import { QuokkaDoc } from '../data/model';
import { normalizeDoc } from '../data/ops';
import { Store } from '../data/store';
import { ToastService } from '../ui/toast.service';
import { PUSH_DEBOUNCE_MS } from './config';
import { SyncAction, decideSync } from './decide';
import * as drive from './drive';
import { AuthError, GoogleAuth } from './google-auth';

const STATE_KEY = 'quokkapack.driveSync';

interface SavedState {
  connected: boolean;
  fileId?: string;
  /** When this browser last finished a sync. Absent means the next sync is a first sync. */
  lastSyncedAt?: string;
}

export type SyncStatus = 'idle' | 'syncing' | 'reconnect' | 'error';

/** First sync found different data on both sides; the person picks one. */
export interface SyncChoice {
  remote: QuokkaDoc;
}

/**
 * Optional Google Drive sync. Keeps one quokkapack.json in the app's hidden Drive folder and applies
 * last-writer-wins on the whole document, using updatedAt. Pulls on connect, app start and window focus;
 * pushes ~2s after local changes. Local data is never discarded because of a failure.
 */
@Injectable({ providedIn: 'root' })
export class DriveSync {
  private store = inject(Store);
  private auth = inject(GoogleAuth);
  private toast = inject(ToastService);

  private saved = signal<SavedState>(readState());
  readonly connected = signal(this.saved().connected);
  readonly status = signal<SyncStatus>('idle');
  readonly error = signal<string | null>(null);
  readonly lastSyncedAt = signal<string | null>(this.saved().lastSyncedAt ?? null);
  readonly choice = signal<SyncChoice | null>(null);

  private running: Promise<void> | null = null;
  private again = false;
  private pushTimer?: ReturnType<typeof setTimeout>;
  private started = false;

  /** Hooks up listeners and does a silent pull if this browser is connected. Call once after Store.load(). */
  start(): void {
    if (this.started) return;
    this.started = true;
    this.store.onLocalChange(() => this.schedulePush());
    window.addEventListener('focus', () => void this.sync());
    window.addEventListener('online', () => void this.sync());
    void this.sync();
  }

  /** Loads Google's sign-in script ahead of a click, so the popup opens promptly. */
  preload(): void {
    void this.auth.preload().catch(() => undefined);
  }

  /** Connect / Reconnect. Call from a click. */
  async connect(): Promise<void> {
    this.error.set(null);
    try {
      await this.auth.getToken(true);
    } catch (e) {
      this.status.set(this.connected() ? 'reconnect' : 'idle');
      this.error.set((e as Error).message);
      return;
    }
    this.status.set('idle');
    if (!this.connected()) {
      this.connected.set(true);
      this.save({ connected: true });
    }
    await this.sync(true);
  }

  /** Sync now. Call from a click. */
  syncNow(): Promise<void> {
    return this.sync(true);
  }

  /** Resolves a first-sync conflict. */
  async choose(keep: 'drive' | 'local'): Promise<void> {
    await this.sync(true, keep === 'drive' ? 'pull' : 'push');
  }

  async disconnect(): Promise<void> {
    clearTimeout(this.pushTimer);
    await this.auth.revoke().catch(() => undefined);
    this.connected.set(false);
    this.status.set('idle');
    this.error.set(null);
    this.choice.set(null);
    this.lastSyncedAt.set(null);
    this.save({ connected: false }, true);
    this.toast.show('Disconnected from Google Drive. Your data stays in this browser.');
  }

  private schedulePush(): void {
    if (!this.connected()) return;
    clearTimeout(this.pushTimer);
    this.pushTimer = setTimeout(() => void this.sync(), PUSH_DEBOUNCE_MS);
  }

  /** Runs one sync. Calls made while one is running are coalesced into one follow-up run. */
  private sync(interactive = false, force?: 'pull' | 'push'): Promise<void> {
    if (!this.connected()) return Promise.resolve();
    if (this.running) {
      if (!force) {
        this.again = true;
        return this.running;
      }
      return this.running.then(() => this.sync(interactive, force));
    }
    this.running = this.run(interactive, force).finally(() => {
      this.running = null;
      if (this.again) {
        this.again = false;
        void this.sync();
      }
    });
    return this.running;
  }

  private async run(interactive: boolean, force?: 'pull' | 'push'): Promise<void> {
    // Background runs can't open Google's sign-in window; wait for a click on Reconnect.
    if (!interactive && this.status() === 'reconnect' && !this.auth.current()) return;
    if (typeof navigator !== 'undefined' && navigator.onLine === false) {
      this.status.set('error');
      this.error.set('You are offline. Changes will sync when you are back online.');
      return;
    }
    this.status.set('syncing');
    try {
      await this.withToken(interactive, (token) => this.exchange(token, force));
      this.status.set('idle');
      this.error.set(null);
    } catch (e) {
      if (e instanceof AuthError) {
        this.status.set('reconnect');
        this.error.set(interactive ? e.message : 'Google sign-in has expired. Reconnect to keep syncing.');
      } else if (e instanceof drive.NetworkError) {
        this.status.set('error');
        this.error.set('Could not reach Google Drive. Your changes are saved here and will sync later.');
      } else {
        console.error(e);
        this.status.set('error');
        this.error.set((e as Error).message || 'Sync failed');
      }
    }
  }

  /** Runs fn with a token; on a 401 gets a fresh token and retries once. */
  private async withToken(interactive: boolean, fn: (token: string) => Promise<void>): Promise<void> {
    const token = await this.auth.getToken(interactive);
    try {
      await fn(token);
    } catch (e) {
      if (!(e instanceof drive.DriveError) || e.status !== 401) throw e;
      this.auth.invalidate();
      await fn(await this.auth.getToken(interactive));
    }
  }

  private async exchange(token: string, force?: 'pull' | 'push'): Promise<void> {
    let fileId = this.saved().fileId ?? (await drive.findFile(token));
    let raw: unknown = null;
    if (fileId) {
      try {
        raw = await drive.download(token, fileId);
      } catch (e) {
        if (!(e instanceof drive.DriveError) || e.status !== 404) throw e;
        fileId = await drive.findFile(token);
        raw = fileId ? await drive.download(token, fileId) : null;
      }
    }
    let remote: QuokkaDoc | null = null;
    if (raw !== null) {
      try {
        remote = normalizeDoc(raw);
      } catch (e) {
        throw new Error(`The Google Drive copy can’t be used: ${(e as Error).message}`);
      }
    }

    const local = this.store.doc();
    const firstSync = !this.saved().lastSyncedAt;
    let action: SyncAction = force ?? decideSync(local, remote, firstSync);
    if (action === 'pull' && !remote) action = 'push';

    if (action === 'ask') {
      this.choice.set({ remote: remote! });
      return;
    }
    this.choice.set(null);

    if (action === 'pull') {
      const before = local;
      this.store.replace(remote!);
      this.toast.show('Updated from Google Drive', { action: { label: 'Undo', run: () => this.store.restore(before) } });
    } else if (action === 'push') {
      const json = JSON.stringify(local);
      if (fileId) {
        try {
          await drive.update(token, fileId, json);
        } catch (e) {
          if (!(e instanceof drive.DriveError) || e.status !== 404) throw e;
          fileId = await drive.create(token, json);
        }
      } else {
        fileId = await drive.create(token, json);
      }
    }

    const now = new Date().toISOString();
    this.lastSyncedAt.set(now);
    this.save({ connected: true, fileId: fileId ?? undefined, lastSyncedAt: now });
  }

  private save(patch: Partial<SavedState>, reset = false): void {
    const next = reset ? { connected: false, ...patch } : { ...this.saved(), ...patch };
    this.saved.set(next);
    try {
      localStorage.setItem(STATE_KEY, JSON.stringify(next));
    } catch {
      /* storage blocked; sync still works for this visit */
    }
  }
}

function readState(): SavedState {
  try {
    const s = JSON.parse(localStorage.getItem(STATE_KEY) ?? 'null');
    if (s && typeof s === 'object') return { connected: s.connected === true, fileId: s.fileId || undefined, lastSyncedAt: s.lastSyncedAt || undefined };
  } catch {
    /* ignore */
  }
  return { connected: false };
}
