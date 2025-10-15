import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register {
  email = '';
  password = '';
  confirmPassword = '';
  error = '';
  loading = false;

  constructor(private auth: AuthService, private router: Router) {}

  submit() {
    this.error = '';

    // Validation
    if (!this.email || !this.password || !this.confirmPassword) {
      this.error = 'All fields are required';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.error = 'Passwords do not match';
      return;
    }

    if (this.password.length < 6) {
      this.error = 'Password must be at least 6 characters';
      return;
    }

    this.loading = true;
    this.auth.register({
      Email: this.email,
      Password: this.password,
      ConfirmPassword: this.confirmPassword
    })
      .subscribe({
        next: () => {
          this.loading = false;
          this.router.navigateByUrl('/trips');
        },
        error: (e) => {
          this.loading = false;
          // Handle structured error from backend
          if (e?.error?.Errors && Array.isArray(e.error.Errors)) {
            this.error = e.error.Errors.map((err: any) => err.Description || err.description).join('. ');
          } else {
            this.error = e?.error?.message || e?.error || 'Registration failed';
          }
        }
      });
  }
}
