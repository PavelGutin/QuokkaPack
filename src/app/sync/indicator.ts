/**
 * What the sync chip shows.
 * - off: Drive isn't connected; the data lives only in this browser.
 * - unsynced: connected, and there are local changes Drive doesn't have yet.
 * - check: connected, nothing new here, but Drive hasn't been checked since the app opened.
 * - syncing, error: as named.
 * - synced: connected and checked; nothing to upload.
 */
export type SyncIndicator = 'off' | 'unsynced' | 'check' | 'syncing' | 'error' | 'synced';

export interface IndicatorInput {
  connected: boolean;
  syncing: boolean;
  failed: boolean;
  /** This browser has changes that haven't been uploaded. */
  dirty: boolean;
  /** Drive has been read or written since the app opened. */
  checked: boolean;
}

export function syncIndicator(s: IndicatorInput): SyncIndicator {
  if (!s.connected) return 'off';
  if (s.syncing) return 'syncing';
  if (s.failed) return 'error';
  if (s.dirty) return 'unsynced';
  return s.checked ? 'synced' : 'check';
}

/** Local changes not yet in Drive: the document's stamp differs from the one last synced. */
export function isDirty(localUpdatedAt: string, syncedUpdatedAt: string | undefined): boolean {
  return !!localUpdatedAt && localUpdatedAt !== syncedUpdatedAt;
}
