import { NgTemplateOutlet } from '@angular/common';
import { Component, computed, inject, input, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import * as ops from '../../data/ops';
import { STARTER_CATALOG } from '../../data/starter';
import { Store } from '../../data/store';
import { DriveSync } from '../../sync/drive-sync.service';
import { hasData } from '../../sync/decide';
import { ToastService } from '../../ui/toast.service';
import { needsWelcome } from './needs-welcome';

/** start → (new) storage → local? → starter;  start → (returning) return → found | notfound. */
export type WelcomeStep = 'start' | 'storage' | 'local' | 'starter' | 'return' | 'found' | 'notfound';
const STEPS: WelcomeStep[] = ['start', 'storage', 'local', 'starter', 'return', 'found', 'notfound'];

@Component({
  selector: 'qp-welcome',
  imports: [NgTemplateOutlet],
  templateUrl: './welcome.html',
})
export class WelcomePage implements OnInit {
  store = inject(Store);
  sync = inject(DriveSync);
  private router = inject(Router);
  private toast = inject(ToastService);

  /** ?step= keeps the browser Back button working through the flow. */
  stepParam = input<string>(undefined, { alias: 'step' });
  step = computed<WelcomeStep>(() => (STEPS as string[]).includes(this.stepParam() ?? '') ? (this.stepParam() as WelcomeStep) : 'start');

  readonly starterNames = Object.keys(STARTER_CATALOG).sort();
  readonly starterCount = Object.values(STARTER_CATALOG).reduce((n, l) => n + l.length, 0);

  busy = signal(false);
  persist = signal<'unknown' | 'granted' | 'declined'>('unknown');

  savedAt = computed(() => {
    const at = this.store.doc().updatedAt;
    return at ? new Date(at).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' }) : '';
  });

  ngOnInit() {
    // Someone who already has lists, or is syncing, never needs this. Found is the one step that follows a pull.
    if (this.step() !== 'found' && this.step() !== 'starter' && !needsWelcome(this.store, this.sync)) {
      this.router.navigate(['/'], { replaceUrl: true });
      return;
    }
    this.sync.preload();
    navigator.storage?.persisted?.().then((p) => p && this.persist.set('granted')).catch(() => undefined);
  }

  go(step: WelcomeStep, replace = false) {
    this.router.navigate(['/welcome'], { queryParams: { step: step === 'start' ? null : step }, replaceUrl: replace });
  }

  /** Connect Drive. A Drive copy with data lands on Found; otherwise new people continue, returning people see Not found. */
  async connect(returning: boolean, chooseAccount = false) {
    if (this.busy()) return;
    this.busy.set(true);
    try {
      if (chooseAccount && this.sync.connected()) await this.sync.disconnect(true);
      const ok = await this.sync.connect(chooseAccount);
      if (!ok) return;
      if (hasData(this.store.doc())) this.go('found', true);
      else this.go(returning ? 'notfound' : 'starter', returning && this.step() === 'notfound');
    } finally {
      this.busy.set(false);
    }
  }

  async checkAgain() {
    if (this.busy()) return;
    this.busy.set(true);
    try {
      await this.sync.syncNow();
      if (hasData(this.store.doc())) this.go('found', true);
      else if (!this.sync.error()) this.toast.show('Still nothing in this Google Drive');
    } finally {
      this.busy.set(false);
    }
  }

  async askPersist() {
    const ok = (await navigator.storage?.persist?.().catch(() => false)) ?? false;
    this.persist.set(ok ? 'granted' : 'declined');
  }

  async restore(input: HTMLInputElement) {
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
    if (this.store.update(() => ({ ...doc, onboarded: true }))) {
      this.toast.show(`Restored ${doc.trips.length} trips and ${doc.items.length} items`, { kind: 'success' });
      this.router.navigate(['/'], { replaceUrl: true });
    }
  }

  useStarter() {
    this.store.update((d) => ops.addStarterCatalog(d));
    this.router.navigate(['/'], { replaceUrl: true });
  }

  startEmpty() {
    this.store.update((d) => ops.apply(d, (x) => (x.onboarded = true)));
    this.router.navigate(['/'], { replaceUrl: true });
  }

  finish() {
    if (!this.store.doc().onboarded) this.store.update((d) => ops.apply(d, (x) => (x.onboarded = true)));
    this.router.navigate(['/'], { replaceUrl: true });
  }
}
