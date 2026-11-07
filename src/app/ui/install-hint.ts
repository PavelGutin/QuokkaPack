/**
 * How to install when the browser can't show its own install dialog on request.
 * - ios: Safari and other iPhone/iPad browsers, via Share → Add to Home Screen.
 * - firefox-android: Firefox's menu has Install / Add to Home screen.
 * - unsupported: Firefox on a computer can't install web apps.
 * - menu: everything else (Chrome, Edge, Samsung Internet) has it in the browser menu.
 */
export type InstallHint = 'ios' | 'firefox-android' | 'unsupported' | 'menu';

export function installHint(userAgent: string, maxTouchPoints = 0): InstallHint {
  const ua = userAgent || '';
  const iPadAsMac = /Macintosh/.test(ua) && maxTouchPoints > 1;
  if (/iPhone|iPad|iPod/.test(ua) || iPadAsMac) return 'ios';
  if (/Firefox\//.test(ua)) return /Android/.test(ua) ? 'firefox-android' : 'unsupported';
  return 'menu';
}

export const INSTALL_HINT_TEXT: Record<InstallHint, string> = {
  ios: 'Tap the Share button, then Add to Home Screen.',
  'firefox-android': 'Open the browser menu (⋮) and tap Install or Add to Home screen.',
  unsupported: 'Firefox on a computer can’t install apps. Open QuokkaPack in Chrome or Edge to install it.',
  menu: 'Open the browser menu (⋮) and choose Install app or Add to Home screen. If it’s already installed, open it from your home screen or app list.',
};
