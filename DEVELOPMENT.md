# Developing QuokkaPack

QuokkaPack is a static Angular 20 app (standalone components, signals) with Bootstrap 5 and its own "Trailhead"
styling. There is no server: data lives in the browser, with optional sync to the person's Google Drive.

## Running it

Requires Node 22+.

```bash
npm install
npm start        # dev server at http://localhost:4200
npm test         # unit tests (vitest)
npm run build    # production build in dist/quokkapack/browser
```

The production build includes a service worker for offline use. The dev server does not.

## Deploying

Pull requests run the tests and build, and keep the built site for a week as the `site` artifact. Pushing to
`master` runs the tests, builds the app, and publishes it to GitHub Pages
(`.github/workflows/pages.yml`, "Build and deploy"). Pages is set to **Source: GitHub Actions**. The build is a
static folder, so any static host works.

## Data

Everything is one JSON document (`QuokkaDoc`, `src/app/data/model.ts`) held by the Store
(`src/app/data/store.ts`) and saved to IndexedDB. Open tabs stay in sync.

- Every change is a pure function `(doc) => doc` in `src/app/data/ops.ts`, unit-tested in `ops.test.ts`.
- Every local change stamps `doc.updatedAt` and notifies `Store.onLocalChange` listeners. `Store.replace()` swaps in a
  document from elsewhere (sync) without restamping it; `Store.restore()` brings back an earlier one as a new local
  change.
- `normalizeDoc()` in `ops.ts` validates untrusted data (backup files, the Drive copy).

## Google Drive sync

Code is in `src/app/sync`.

- **Storage:** one `quokkapack.json` in the hidden `appDataFolder` of the person's Drive. The only scope is
  `https://www.googleapis.com/auth/drive.appdata`.
- **Auth:** Google Identity Services, loaded on demand, with `initTokenClient` (`google-auth.ts`). Access tokens
  live in memory only. Disconnect revokes the token.
- **API:** Drive REST v3 called with `fetch`, no gapi (`drive.ts`). List by name, multipart create,
  `PATCH uploadType=media` update.
- **Rule:** last writer wins on the whole document by `updatedAt` (`decide.ts`). On the first sync in a browser where
  both sides already hold data, the person picks which copy to keep. A pull shows an Undo toast unless the browser
  was empty.
- **When it runs** (`drive-sync.service.ts`):
  - Interactive syncs (Connect, Sync now, the sync chip) can open Google's window because they follow a tap.
  - Background syncs (app start, window focus, coming back online, about 2 seconds after local changes) run only while
    a token is in memory. Getting a new token always opens a Google window, and phones block windows that don't
    follow a tap.
  - With **Sync automatically** on, background syncs request a token anyway, which may cause an "Allow this site to
    open?" prompt.
- **Status:** `indicator.ts` turns sync state into the chip's states (off, unsynced, check, syncing, error,
  synced). Unsynced means the local `updatedAt` differs from the one saved at the end of the last sync.
- **Settings:** the OAuth client ID is in `src/app/sync/config.ts`. It is public by design; there is no client
  secret. The Google Cloud project is `quokkapack-pbg`, with a Web OAuth client whose authorized JavaScript origins
  are `https://pavelgutin.github.io` and `http://localhost:4200`.
- Local sync state (connected, file id, last synced, auto) is in `localStorage` under `quokkapack.driveSync`.
- The service worker only caches the app's own files, so Google requests are never cached.

## Theme

- Color tokens are CSS custom properties on `:root` and `[data-bs-theme='dark']` in `src/styles.scss`, followed by the
  Trailhead shape layer (outlines, offset shadows, serif headings).
- `ui/theme.ts` follows `prefers-color-scheme` unless the person picks Light or Dark (stored under
  `quokkapack.theme`). An inline script in `index.html` applies the same rule before Angular starts, so there's no
  flash of the wrong theme.
- Fonts are bundled or system fonts. Nothing is loaded from a third party, which the privacy policy relies on.

## Layout

| Path | What |
| --- | --- |
| `src/app/data/` | Document types, operations, store, starter catalog |
| `src/app/sync/` | Google Drive sync: auth, Drive client, decision rule, status |
| `src/app/pages/welcome/` | First-run flow: new vs returning, storage choice, starter catalog |
| `src/app/pages/trips/` | Trips home: next-trip header and the list of other trips |
| `src/app/pages/trip/` | One trip: Pack and Choose items modes |
| `src/app/pages/trip-new/` | New trip from categories or a copy of another trip |
| `src/app/pages/catalog/` | Catalog editing |
| `src/app/pages/backup/` | Sync & settings |
| `src/app/ui/` | Shared pieces: sync chip, theme, toasts, formatting, inline edit |
| `public/privacy.html` | Privacy policy |
| `public/icons/` | App icons (regular and maskable), Apple touch icon, favicon, and the transparent `quokka.png` used on the welcome screen and README |
| `docs/screenshots/` | README screenshots, taken at phone width from the live site with demo data |
