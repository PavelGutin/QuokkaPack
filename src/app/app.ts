import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Store } from './data/store';
import { ToastService } from './ui/toast.service';

@Component({
  selector: 'qp-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
})
export class App {
  store = inject(Store);
  toasts = inject(ToastService);
  menuOpen = signal(false);
}
