import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  message: string;
  kind: 'dark' | 'danger' | 'success';
  action?: { label: string; run: () => void };
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  readonly toasts = signal<Toast[]>([]);
  private next = 1;

  show(message: string, opts: { kind?: Toast['kind']; action?: Toast['action']; ms?: number } = {}): void {
    const toast: Toast = { id: this.next++, message, kind: opts.kind ?? 'dark', action: opts.action };
    this.toasts.update((t) => [...t.slice(-2), toast]);
    setTimeout(() => this.dismiss(toast.id), opts.ms ?? (opts.action ? 7000 : 3500));
  }

  dismiss(id: number): void {
    this.toasts.update((t) => t.filter((x) => x.id !== id));
  }
}
