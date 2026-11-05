import { Injectable, computed, inject, signal } from '@angular/core';
import { QuokkaDoc, emptyDoc } from './model';
import { normalizeDoc } from './ops';
import { idbGet, idbSet } from './idb';
import { ToastService } from '../ui/toast.service';

const KEY = 'doc';

/**
 * Holds the whole document in a signal and persists every change to IndexedDB.
 * Other open tabs are told to reload via BroadcastChannel.
 */
@Injectable({ providedIn: 'root' })
export class Store {
  private toast = inject(ToastService);
  private readonly _doc = signal<QuokkaDoc>(emptyDoc());
  private channel = typeof BroadcastChannel !== 'undefined' ? new BroadcastChannel('quokkapack') : null;
  private saving: Promise<void> = Promise.resolve();

  readonly doc = this._doc.asReadonly();
  readonly ready = signal(false);
  /** Set when storage is unavailable (e.g. some private-browsing modes). */
  readonly storageError = signal<string | null>(null);

  readonly categories = computed(() => this.doc().categories);
  readonly items = computed(() => this.doc().items);
  readonly trips = computed(() => this.doc().trips);

  async load(): Promise<void> {
    try {
      const stored = await idbGet<QuokkaDoc>(KEY);
      if (stored) this._doc.set(normalizeDoc(stored));
      navigator.storage?.persist?.().catch(() => undefined);
    } catch (e) {
      console.error(e);
      this.storageError.set('Your browser is blocking local storage, so changes will not be saved.');
    }
    this.channel?.addEventListener('message', async () => {
      const stored = await idbGet<QuokkaDoc>(KEY).catch(() => undefined);
      if (stored) this._doc.set(normalizeDoc(stored));
    });
    this.ready.set(true);
  }

  /**
   * Applies a pure operation. Errors thrown by the operation are shown as a toast and the doc is untouched.
   * Pass `undo` to show a toast with an Undo button.
   */
  update(op: (d: QuokkaDoc) => QuokkaDoc, undo?: string): boolean {
    const before = this._doc();
    let after: QuokkaDoc;
    try {
      after = op(before);
    } catch (e) {
      this.toast.show((e as Error).message, { kind: 'danger' });
      return false;
    }
    if (after === before) return true;
    this.commit(after);
    if (undo) this.toast.show(undo, { action: { label: 'Undo', run: () => this.commit(before) } });
    return true;
  }

  /** Replaces the whole document (import / erase). */
  replace(doc: QuokkaDoc): void {
    this.commit(doc);
  }

  private commit(doc: QuokkaDoc): void {
    this._doc.set(doc);
    this.saving = this.saving
      .then(() => idbSet(KEY, doc))
      .then(() => this.channel?.postMessage('changed'))
      .catch((e) => {
        console.error(e);
        this.storageError.set('Could not save your changes to this browser’s storage.');
      });
  }
}
