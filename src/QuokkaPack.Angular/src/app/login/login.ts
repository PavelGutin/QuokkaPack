// src/app/login/login.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <h2>Login</h2>
    <form (ngSubmit)="submit()">
      <label>Email
        <input [(ngModel)]="email" name="email" required />
      </label>
      <label>Password
        <input [(ngModel)]="password" name="password" type="password" required />
      </label>
      <button type="submit">Sign in</button>
      <p *ngIf="error" style="color:red">{{ error }}</p>
    </form>
  `
})
export class Login {
  email = '';
  password = '';
  error = '';

  constructor(private auth: AuthService, private router: Router) {}

  submit() {
    this.error = '';
    this.auth.login({ email: this.email, password: this.password })
      .subscribe({
        next: () => this.router.navigateByUrl('/'),
        error: (e) => this.error = e?.error?.message ?? 'Login failed'
      });
  }
}
