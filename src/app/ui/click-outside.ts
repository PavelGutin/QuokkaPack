import { Directive, ElementRef, HostListener, inject, output } from '@angular/core';

@Directive({ selector: '[qpClickOutside]' })
export class ClickOutside {
  private el = inject(ElementRef);
  qpClickOutside = output<void>();

  @HostListener('document:click', ['$event'])
  onClick(e: Event) {
    if (!this.el.nativeElement.contains(e.target)) this.qpClickOutside.emit();
  }

  @HostListener('document:keydown.escape')
  onEsc() {
    this.qpClickOutside.emit();
  }
}
