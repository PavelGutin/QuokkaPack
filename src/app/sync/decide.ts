import { QuokkaDoc } from '../data/model';

/**
 * pull: replace local with the Drive copy. push: upload local. noop: already in step.
 * ask: first sync on this browser and both copies hold different data, so the person picks which to keep.
 */
export type SyncAction = 'pull' | 'push' | 'noop' | 'ask';

type Doc = Pick<QuokkaDoc, 'updatedAt' | 'trips' | 'items' | 'categories'>;

export function hasData(doc: Doc): boolean {
  return doc.trips.length > 0 || doc.items.length > 0 || doc.categories.length > 0;
}

/** Milliseconds since epoch, or -Infinity for a missing or unparseable stamp. */
function stamp(iso: string): number {
  const t = Date.parse(iso);
  return Number.isNaN(t) ? -Infinity : t;
}

/**
 * Last writer wins on the whole document, by updatedAt.
 * @param firstSync true when this browser has never synced with Drive (fresh connect). Then an empty side
 *   always takes the other side's data, and two non-empty copies are not overwritten without asking.
 */
export function decideSync(local: Doc, remote: Doc | null, firstSync: boolean): SyncAction {
  if (!remote) return hasData(local) || local.updatedAt ? 'push' : 'noop';
  const l = stamp(local.updatedAt);
  const r = stamp(remote.updatedAt);
  if (firstSync) {
    if (!hasData(local) && hasData(remote)) return 'pull';
    if (hasData(local) && !hasData(remote)) return 'push';
    if (l === r) return 'noop';
    if (hasData(local) && hasData(remote)) return 'ask';
  }
  if (l === r) return 'noop';
  return r > l ? 'pull' : 'push';
}
