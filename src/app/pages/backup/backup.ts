import { Component, inject, OnInit, signal } from '@angular/core';
import { emptyDoc } from '../../data/model';
import * as ops from '../../data/ops';
import { Store } from '../../data/store';
import { ToastService } from '../../ui/toast.service';

@Component({
  selector: 'qp-backup',
  templateUrl: './backup.html',
})
export class BackupPage implements OnInit {
  store = inject(Store);
  private toast = inject(ToastService);
  persisted = signal<boolean | null>(null);

  async ngOnInit() {
    try {
      this.persisted.set((await navigator.storage?.persisted?.()) ?? null);
    } catch {
      /* unsupported */
    }
  }

  async requestPersist() {
    const ok = (await navigator.storage?.persist?.().catch(() => false)) ?? false;
    this.persisted.set(ok);
    this.toast.show(ok ? 'This browser will keep your data' : 'The browser declined. Keep a backup to be safe.', { kind: ok ? 'success' : 'dark' });
  }

  export() {
    const blob = new Blob([ops.toBackup(this.store.doc())], { type: 'application/json' });
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = `quokkapack-${ops.todayIso()}.json`;
    a.click();
    setTimeout(() => URL.revokeObjectURL(a.href), 1000);
  }

  async import(input: HTMLInputElement) {
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;
    let doc;
    try {
      doc = ops.fromBackup(await file.text());
    } catch (e) {
      this.toast.show((e as Error).message, { kind: 'danger' });
      return;
    }
    const msg = `Replace everything here with ${doc.trips.length} trips and ${doc.items.length} items from “${file.name}”?`;
    if (!confirm(msg)) return;
    this.store.update(() => doc, 'Backup restored');
  }

  addStarter() {
    const before = this.store.items().length;
    this.store.update((d) => ops.addStarterCatalog(d), 'Starter catalog added');
    if (this.store.items().length === before) this.toast.show('You already have all the starter items');
  }

  eraseAll() {
    if (!confirm('Delete all trips, categories and items from this browser?')) return;
    this.store.update(() => ({ ...emptyDoc(), onboarded: true, updatedAt: new Date().toISOString() }), 'Everything was erased');
  }
}
