import { Directive, EventEmitter, HostListener, Input, Output } from '@angular/core';

@Directive({ selector: '[appColumnResize]', standalone: true })
export class ColumnResizeDirective {
  @Input({ required: true }) appColumnResize = 0;
  @Output() readonly widthChange = new EventEmitter<number>();
  private active = false;
  private startX = 0;
  private startWidth = 0;

  @HostListener('mousedown', ['$event'])
  start(event: MouseEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.active = true;
    this.startX = event.clientX;
    this.startWidth = this.appColumnResize;
  }

  @HostListener('document:mousemove', ['$event'])
  resize(event: MouseEvent): void {
    if (this.active) this.widthChange.emit(Math.max(58, this.startWidth + event.clientX - this.startX));
  }

  @HostListener('document:mouseup')
  stop(): void { this.active = false; }
}
