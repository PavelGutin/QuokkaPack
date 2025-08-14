import { Component, signal } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  title = 'QuokkaPack';

 constructor(public auth: AuthService) {}

  get isLoggedIn(): boolean { return this.auth.hasValidToken(); }
  get userName(): string { return this.auth.userName || 'USER'; }

  logout() { this.auth.logout(); }
}
