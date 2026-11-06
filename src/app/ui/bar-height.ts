import { Directive, ElementRef, OnDestroy, OnInit, inject } from '@angular/core';

/** Put on a sticky top bar: keeps --qp-bar-h on its parent in sync with its height, for sticky section headers. */
@Directive({ selector: '[qpBarHeight]' })
export class BarHeight implements OnInit, OnDestroy {
  private el = inject<ElementRef<HTMLElement>>(ElementRef);
  private ro?: ResizeObserver;

  ngOnInit() {
    const bar = this.el.nativeElement;
    const set = () => bar.parentElement?.style.setProperty('--qp-bar-h', `${bar.offsetHeight}px`);
    set();
    if (typeof ResizeObserver !== 'undefined') {
      this.ro = new ResizeObserver(set);
      this.ro.observe(bar);
    }
  }

  ngOnDestroy() {
    this.ro?.disconnect();
  }
}
