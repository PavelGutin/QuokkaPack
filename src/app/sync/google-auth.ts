import { Injectable } from '@angular/core';
import { DRIVE_SCOPE, GIS_SCRIPT_URL, GOOGLE_CLIENT_ID } from './config';

/* Minimal typings for the parts of Google Identity Services used here. */
interface TokenResponse {
  access_token: string;
  expires_in: number;
  error?: string;
  error_description?: string;
}
interface TokenClient {
  requestAccessToken(overrides?: { prompt?: string }): void;
}
interface Oauth2 {
  initTokenClient(config: {
    client_id: string;
    scope: string;
    callback: (r: TokenResponse) => void;
    error_callback?: (e: { type: string; message?: string }) => void;
  }): TokenClient;
  hasGrantedAllScopes(r: TokenResponse, ...scopes: string[]): boolean;
  revoke(token: string, done?: () => void): void;
}
declare global {
  interface Window {
    google?: { accounts: { oauth2: Oauth2 } };
  }
}

export class AuthError extends Error {
  /** True when only an interactive sign-in (a click) can fix it. */
  constructor(message: string, readonly needsInteraction: boolean) {
    super(message);
  }
}

/**
 * Gets Drive access tokens through Google Identity Services. Tokens live in memory only.
 */
@Injectable({ providedIn: 'root' })
export class GoogleAuth {
  private script?: Promise<Oauth2>;
  private client?: TokenClient;
  private pending?: { resolve: (t: string) => void; reject: (e: Error) => void };
  private token: string | null = null;
  private expiresAt = 0;

  /** A token valid for at least another minute, or null. */
  current(): string | null {
    return this.token && Date.now() < this.expiresAt - 60_000 ? this.token : null;
  }

  /** Loads Google's script without requesting anything. */
  async preload(): Promise<void> {
    await this.load();
  }

  /** Forget the in-memory token (e.g. after a 401). */
  invalidate(): void {
    this.token = null;
    this.expiresAt = 0;
  }

  /**
   * Requests a token. interactive=true must be called from a click; it may show Google's account and consent
   * screens. interactive=false asks without a prompt and fails if Google needs the person to act.
   */
  async getToken(interactive: boolean, chooseAccount = false): Promise<string> {
    const cached = chooseAccount ? null : this.current();
    if (cached) return cached;
    const oauth2 = await this.load();
    if (this.pending) this.pending.reject(new AuthError('Superseded', !interactive));
    return new Promise<string>((resolve, reject) => {
      const timer = setTimeout(
        () => this.fail(new AuthError('Google sign-in did not respond', true)),
        interactive ? 180_000 : 20_000,
      );
      this.pending = {
        resolve: (t) => (clearTimeout(timer), resolve(t)),
        reject: (e) => (clearTimeout(timer), reject(e)),
      };
      this.tokenClient(oauth2).requestAccessToken(!interactive ? { prompt: '' } : chooseAccount ? { prompt: 'select_account' } : {});
    });
  }

  /** Revokes the current token, if there is one, and forgets it. */
  async revoke(): Promise<void> {
    const token = this.token;
    this.invalidate();
    if (!token) return;
    const oauth2 = await this.load().catch(() => null);
    await new Promise<void>((done) => (oauth2 ? oauth2.revoke(token, done) : done()));
  }

  private tokenClient(oauth2: Oauth2): TokenClient {
    this.client ??= oauth2.initTokenClient({
      client_id: GOOGLE_CLIENT_ID,
      scope: DRIVE_SCOPE,
      callback: (r) => {
        if (r.error) return this.fail(new AuthError(r.error_description || r.error, true));
        if (!oauth2.hasGrantedAllScopes(r, DRIVE_SCOPE)) {
          return this.fail(new AuthError('Google Drive access was not granted', true));
        }
        this.token = r.access_token;
        this.expiresAt = Date.now() + Number(r.expires_in) * 1000;
        const p = this.pending;
        this.pending = undefined;
        p?.resolve(r.access_token);
      },
      error_callback: (e) => {
        const msg =
          e.type === 'popup_closed'
            ? 'The Google sign-in window was closed'
            : e.type === 'popup_failed_to_open'
              ? 'The browser blocked the Google sign-in window'
              : e.message || 'Google sign-in failed';
        this.fail(new AuthError(msg, true));
      },
    });
    return this.client;
  }

  private fail(e: Error): void {
    const p = this.pending;
    this.pending = undefined;
    p?.reject(e);
  }

  private load(): Promise<Oauth2> {
    this.script ??= new Promise<Oauth2>((resolve, reject) => {
      if (window.google?.accounts?.oauth2) return resolve(window.google.accounts.oauth2);
      const s = document.createElement('script');
      s.src = GIS_SCRIPT_URL;
      s.async = true;
      s.onload = () =>
        window.google?.accounts?.oauth2 ? resolve(window.google.accounts.oauth2) : reject(new Error('Google sign-in failed to load'));
      s.onerror = () => reject(new Error('Could not load Google sign-in. Are you offline?'));
      document.head.appendChild(s);
    }).catch((e) => {
      this.script = undefined;
      throw e;
    });
    return this.script;
  }
}
