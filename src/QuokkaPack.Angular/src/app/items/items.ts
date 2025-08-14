import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-items',
  imports: [CommonModule],
  template: `<h2>Items</h2><p>Your items will appear here.</p>`
})
export class Items {}
