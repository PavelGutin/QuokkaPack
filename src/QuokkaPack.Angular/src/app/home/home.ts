import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  standalone: true,
  selector: 'app-home',
  imports: [CommonModule],
  template: `
    <p>Welcome to QuokkaPack</p>
  `
})
export class Home {}
