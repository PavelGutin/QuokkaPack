import { Component, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './core/auth/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  title = 'QuokkaPack';
  mobileMenuOpen = false;

 constructor(public auth: AuthService, private router: Router) {}

  get isLoggedIn(): boolean { return this.auth.hasValidToken(); }
  get userName(): string { return this.auth.userName || 'USER'; }

  toggleMobileMenu() {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  closeMobileMenu() {
    this.mobileMenuOpen = false;
  }

  logout() {
    this.auth.logout();
    this.mobileMenuOpen = false;
    this.router.navigate(['/']);
  }
}
