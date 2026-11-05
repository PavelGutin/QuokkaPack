import { describe, expect, it } from 'vitest';
import { QuokkaDoc, emptyDoc } from '../data/model';
import { decideSync } from './decide';

const OLD = '2026-01-01T00:00:00.000Z';
const NEW = '2026-02-01T00:00:00.000Z';

function doc(updatedAt: string, withData = true): QuokkaDoc {
  const d = { ...emptyDoc(), updatedAt };
  if (withData) d.categories = [{ id: 'c', name: 'Clothing', archived: false }];
  return d;
}

describe('decideSync', () => {
  it('pushes when Drive has no copy and local has data', () => {
    expect(decideSync(doc(OLD), null, true)).toBe('push');
    expect(decideSync(doc(OLD), null, false)).toBe('push');
  });

  it('does nothing when neither side has anything', () => {
    expect(decideSync(emptyDoc(), null, true)).toBe('noop');
  });

  it('pulls when the Drive copy is newer', () => {
    expect(decideSync(doc(OLD), doc(NEW), false)).toBe('pull');
  });

  it('pushes when the local copy is newer', () => {
    expect(decideSync(doc(NEW), doc(OLD), false)).toBe('push');
  });

  it('does nothing when both copies have the same stamp', () => {
    expect(decideSync(doc(NEW), doc(NEW), false)).toBe('noop');
    expect(decideSync(doc(NEW), doc(NEW), true)).toBe('noop');
  });

  it('treats a missing or bad stamp as oldest', () => {
    expect(decideSync(doc(''), doc(OLD), false)).toBe('pull');
    expect(decideSync(doc(OLD), doc('garbage'), false)).toBe('push');
  });

  it('propagates an erase from another device', () => {
    expect(decideSync(doc(OLD), doc(NEW, false), false)).toBe('pull');
  });

  describe('first sync on this browser', () => {
    it('pulls into an empty browser even if its stamp is newer', () => {
      expect(decideSync(doc(NEW, false), doc(OLD), true)).toBe('pull');
    });

    it('pushes over an empty Drive copy even if its stamp is newer', () => {
      expect(decideSync(doc(OLD), doc(NEW, false), true)).toBe('push');
    });

    it('asks when both sides hold different data', () => {
      expect(decideSync(doc(NEW), doc(OLD), true)).toBe('ask');
      expect(decideSync(doc(OLD), doc(NEW), true)).toBe('ask');
    });
  });
});
