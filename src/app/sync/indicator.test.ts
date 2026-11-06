import { describe, expect, it } from 'vitest';
import { isDirty, syncIndicator } from './indicator';

const base = { connected: true, syncing: false, failed: false, dirty: false, checked: true };

describe('syncIndicator', () => {
  it('is off when Drive is not connected', () => {
    expect(syncIndicator({ ...base, connected: false, dirty: true })).toBe('off');
  });

  it('shows unsynced when there are local changes', () => {
    expect(syncIndicator({ ...base, dirty: true })).toBe('unsynced');
    expect(syncIndicator({ ...base, dirty: true, checked: false })).toBe('unsynced');
  });

  it('asks for a check when Drive has not been read this session', () => {
    expect(syncIndicator({ ...base, checked: false })).toBe('check');
  });

  it('is synced only when checked and clean', () => {
    expect(syncIndicator(base)).toBe('synced');
  });

  it('puts syncing and errors ahead of everything else', () => {
    expect(syncIndicator({ ...base, dirty: true, syncing: true })).toBe('syncing');
    expect(syncIndicator({ ...base, dirty: true, failed: true })).toBe('error');
  });
});

describe('isDirty', () => {
  it('compares the local stamp to the last synced one', () => {
    expect(isDirty('2026-11-06T10:00:00.000Z', '2026-11-06T10:00:00.000Z')).toBe(false);
    expect(isDirty('2026-11-06T10:05:00.000Z', '2026-11-06T10:00:00.000Z')).toBe(true);
    expect(isDirty('2026-11-06T10:05:00.000Z', undefined)).toBe(true);
    expect(isDirty('', undefined)).toBe(false);
  });
});
