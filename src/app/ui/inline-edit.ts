import { Component, ElementRef, effect, input, output, signal, viewChild } from '@angular/core';

/** Text that turns into an input when clicked. Enter/blur saves, Escape cancels. */
@Component({
  selector: 'qp-inline-edit',
  template: `
    @if (editing()) {
      <input #box class="form-control form-control-sm inline-edit-input" [value]="value()" [attr.aria-label]="label()"
        (keydown.enter)="commit(box.value)" (keydown.escape)="editing.set(false)" (blur)="commit(box.value)" (click)="$event.stopPropagation()" />
    } @else {
      <button type="button" class="inline-edit-text" [title]="'Rename ' + value()" (click)="start($event)">{{ value() }}</button>
    }
  `,
})
export class InlineEdit {
  value = input.required<string>();
  label = input('Name');
  save = output<string>();
  editing = signal(false);
  private box = viewChild<ElementRef<HTMLInputElement>>('box');

  constructor() {
    effect(() => {
      const el = this.box()?.nativeElement;
      if (el && document.activeElement !== el) {
        el.focus();
        el.select();
      }
    });
  }

  start(e: Event) {
    e.stopPropagation();
    this.editing.set(true);
  }

  commit(v: string) {
    if (!this.editing()) return;
    this.editing.set(false);
    if (v.trim() && v.trim() !== this.value()) this.save.emit(v);
  }
}
