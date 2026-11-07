import { describe, expect, it } from 'vitest';
import { installHint } from './install-hint';

const UA = {
  iphone: 'Mozilla/5.0 (iPhone; CPU iPhone OS 18_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.1 Mobile/15E148 Safari/604.1',
  ipadDesktop: 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.1 Safari/605.1.15',
  androidChrome: 'Mozilla/5.0 (Linux; Android 14; SM-S918B) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Mobile Safari/537.36',
  androidFirefox: 'Mozilla/5.0 (Android 14; Mobile; rv:132.0) Gecko/132.0 Firefox/132.0',
  desktopFirefox: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:132.0) Gecko/20100101 Firefox/132.0',
  desktopChrome: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36',
};

describe('installHint', () => {
  it('sends iPhone and iPad to Add to Home Screen', () => {
    expect(installHint(UA.iphone)).toBe('ios');
    expect(installHint(UA.ipadDesktop, 5)).toBe('ios');
  });

  it('treats a real Mac as a desktop browser', () => {
    expect(installHint(UA.ipadDesktop, 0)).toBe('menu');
  });

  it('splits Firefox by platform', () => {
    expect(installHint(UA.androidFirefox)).toBe('firefox-android');
    expect(installHint(UA.desktopFirefox)).toBe('unsupported');
  });

  it('uses the browser menu for Chromium browsers', () => {
    expect(installHint(UA.androidChrome)).toBe('menu');
    expect(installHint(UA.desktopChrome)).toBe('menu');
  });
});
