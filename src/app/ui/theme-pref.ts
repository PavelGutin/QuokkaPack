export type ThemePref = 'system' | 'light' | 'dark';
export type Theme = 'light' | 'dark';

export const THEME_KEY = 'quokkapack.theme';

/** Browser bar colors, matching --qp-bar in styles.scss. */
export const THEME_COLOR: Record<Theme, string> = { light: '#F3EBDD', dark: '#161A16' };

export function parsePref(v: unknown): ThemePref {
  return v === 'light' || v === 'dark' ? v : 'system';
}

export function resolveTheme(pref: ThemePref, systemDark: boolean): Theme {
  if (pref === 'system') return systemDark ? 'dark' : 'light';
  return pref;
}
