import { SYNC_FILE_NAME } from './config';

/** Drive REST v3, called directly with fetch. Everything lives in the hidden appDataFolder. */

const API = 'https://www.googleapis.com/drive/v3/files';
const UPLOAD = 'https://www.googleapis.com/upload/drive/v3/files';

export class DriveError extends Error {
  constructor(
    message: string,
    readonly status: number,
  ) {
    super(message);
  }
}

/** Thrown when the request never reached Google (offline, DNS, CORS failure). */
export class NetworkError extends Error {}

async function call(token: string, url: string, init: RequestInit = {}): Promise<Response> {
  let res: Response;
  try {
    res = await fetch(url, {
      ...init,
      cache: 'no-store',
      headers: { ...(init.headers as Record<string, string>), Authorization: `Bearer ${token}` },
    });
  } catch {
    throw new NetworkError('Could not reach Google Drive');
  }
  if (!res.ok) {
    let detail = '';
    try {
      detail = (await res.json())?.error?.message ?? '';
    } catch {
      /* not JSON */
    }
    throw new DriveError(detail || `Google Drive returned ${res.status}`, res.status);
  }
  return res;
}

/** Id of the newest quokkapack.json in appDataFolder, or null. */
export async function findFile(token: string): Promise<string | null> {
  const q = encodeURIComponent(`name = '${SYNC_FILE_NAME}' and trashed = false`);
  const fields = encodeURIComponent('files(id,modifiedTime)');
  const res = await call(token, `${API}?spaces=appDataFolder&q=${q}&fields=${fields}&orderBy=modifiedTime%20desc&pageSize=10`);
  const body = (await res.json()) as { files?: { id: string }[] };
  return body.files?.[0]?.id ?? null;
}

export async function download(token: string, id: string): Promise<unknown> {
  const res = await call(token, `${API}/${encodeURIComponent(id)}?alt=media`);
  const text = await res.text();
  try {
    return JSON.parse(text);
  } catch {
    throw new DriveError('The Google Drive copy is not valid JSON', 0);
  }
}

/** Creates quokkapack.json in appDataFolder and returns its id. */
export async function create(token: string, json: string): Promise<string> {
  const boundary = 'qp' + Math.random().toString(36).slice(2);
  const meta = JSON.stringify({ name: SYNC_FILE_NAME, parents: ['appDataFolder'], mimeType: 'application/json' });
  const body =
    `--${boundary}\r\nContent-Type: application/json; charset=UTF-8\r\n\r\n${meta}\r\n` +
    `--${boundary}\r\nContent-Type: application/json\r\n\r\n${json}\r\n--${boundary}--`;
  const res = await call(token, `${UPLOAD}?uploadType=multipart&fields=id`, {
    method: 'POST',
    headers: { 'Content-Type': `multipart/related; boundary=${boundary}` },
    body,
  });
  return ((await res.json()) as { id: string }).id;
}

/** Replaces the file's contents. */
export async function update(token: string, id: string, json: string): Promise<void> {
  await call(token, `${UPLOAD}/${encodeURIComponent(id)}?uploadType=media&fields=id`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    body: json,
  });
}
