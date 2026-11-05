# 🦘📦 QuokkaPack

Packing lists for every trip. Keep a catalog of everything you might bring, pick from it for each trip, and check
things off as you pack.

It runs entirely in the browser. There is no server and no account, and it works offline once loaded. On a phone,
use "Add to Home Screen" to install it like an app.

## Features

- **Trips** with optional dates, notes, a countdown and a packing progress bar. Upcoming trips come first; past
  trips are folded away.
- **Pack mode:** check items off by category, search, hide packed items, or check off a whole category at once.
- **Choose items mode:** add or remove items or whole categories, set quantities, and add new items on the spot.
  New items are saved to the catalog too.
- **Start a trip from categories** or **copy a previous trip**. You can also duplicate any trip.
- **Catalog:** rename by clicking, move items between categories, and archive what you no longer use. Archived
  items stay on old trips but are hidden from new ones.
- **Undo** for deletes, moves and resets.
- **Backup:** download everything as one JSON file and restore it in any browser. First-time users can load a
  starter catalog.

## Where data lives

Everything is one JSON document stored in the browser's IndexedDB (`src/app/data`). Open tabs stay in sync. Each
browser keeps its own copy, so use **Backup** to move data between devices. Syncing across devices (for example
through Google Drive) is planned. Because the data is a single document, that will mostly mean storing that one
file somewhere.

## Development

Requires Node 22+.

```bash
npm install
```

Start the dev server at http://localhost:4200:

```bash
npm start
```

Run the unit tests for the data layer:

```bash
npm test
```

Make a production build in `dist/quokkapack/browser`:

```bash
npm run build
```

The production build includes a service worker for offline use. The dev server does not.

### Layout

| Path | What |
| --- | --- |
| `src/app/data/model.ts` | Document types |
| `src/app/data/ops.ts` | Every change, as a pure function `(doc) => doc` (unit-tested in `ops.test.ts`) |
| `src/app/data/store.ts` | Signal-based store: persistence, undo, cross-tab sync |
| `src/app/pages/*` | Trips, trip, new trip, catalog, backup |

Built with Angular 20 (standalone components, signals) and Bootstrap 5 with the Sandstone theme.

## Deploying

Pushing to `master` runs the tests, builds the app, and publishes it to GitHub Pages
(`.github/workflows/pages.yml`). Enable it once under **Settings → Pages → Source: GitHub Actions**. The build is a
static folder, so any static host works.

## License

MIT
