import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Store } from './data/store';
import { ThemeService } from './ui/theme';
import { ToastService } from './ui/toast.service';

@Component({
  selector: 'qp-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
})
export class App {
  store = inject(Store);
  toasts = inject(ToastService);
  /** Injected so the theme follows the system from the start. */
  theme = inject(ThemeService);
}
