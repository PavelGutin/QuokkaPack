/** Google Drive sync settings. The OAuth client ID is public by design; there is no client secret in a browser app. */
export const GOOGLE_CLIENT_ID = '494421684581-095putmg7ffu9f515te5cbsrubl7evi4.apps.googleusercontent.com';

/** Only the hidden per-app folder. QuokkaPack cannot see any other Drive files. */
export const DRIVE_SCOPE = 'https://www.googleapis.com/auth/drive.appdata';

export const GIS_SCRIPT_URL = 'https://accounts.google.com/gsi/client';

export const SYNC_FILE_NAME = 'quokkapack.json';

/** Wait this long after the last local change before uploading. */
export const PUSH_DEBOUNCE_MS = 2000;
