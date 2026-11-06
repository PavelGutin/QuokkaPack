import { describe, expect, it } from 'vitest';
import { parsePref, resolveTheme } from './theme-pref';

describe('theme', () => {
  it('follows the system by default', () => {
    expect(resolveTheme('system', true)).toBe('dark');
    expect(resolveTheme('system', false)).toBe('light');
  });

  it('lets a chosen theme override the system', () => {
    expect(resolveTheme('light', true)).toBe('light');
    expect(resolveTheme('dark', false)).toBe('dark');
  });

  it('treats unknown stored values as system', () => {
    expect(parsePref(null)).toBe('system');
    expect(parsePref('blue')).toBe('system');
    expect(parsePref('dark')).toBe('dark');
  });
});
