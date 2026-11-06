import { Store } from '../../data/store';
import { DriveSync } from '../../sync/drive-sync.service';
import { hasData } from '../../sync/decide';

/** True when this browser has nothing yet and isn't syncing, so the welcome flow should run. */
export function needsWelcome(store: Store, sync: DriveSync): boolean {
  const d = store.doc();
  return !d.onboarded && !hasData(d) && !sync.connected();
}
