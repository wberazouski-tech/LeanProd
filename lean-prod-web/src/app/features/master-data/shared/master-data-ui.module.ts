import { AfterViewInit, Directive, ElementRef, EventEmitter, HostBinding, HostListener, NgModule, OnDestroy, OnInit, Output, inject } from '@angular/core';
import { FormFieldErrorComponent } from '../../../core/ui/form-field-error.component';
import { PageStateComponent } from '../../../core/ui/page-state.component';

@Directive({ selector: '[appKeyboardRow]', standalone: true })
export class KeyboardRowDirective implements OnInit {
  @HostBinding('attr.tabindex') readonly tabIndex = 0;
  private readonly element = inject<ElementRef<HTMLElement>>(ElementRef);
  ngOnInit(): void { const element = this.element.nativeElement; if (element.tagName !== 'TR' && !element.hasAttribute('role')) element.setAttribute('role', 'button'); }
  @HostListener('keydown', ['$event']) activate(event: KeyboardEvent): void {
    if (event.target !== this.element.nativeElement || !['Enter', ' '].includes(event.key)) return;
    event.preventDefault();
    this.element.nativeElement.click();
  }
}

@Directive({ selector: 'dialog[appModal]', standalone: true })
export class MasterDataModalDirective implements AfterViewInit, OnDestroy {
  private readonly element = inject<ElementRef<HTMLDialogElement>>(ElementRef);
  private readonly previousFocus = document.activeElement as HTMLElement | null;
  private static nextId = 0;
  @Output() readonly appModalClosed = new EventEmitter<void>();
  ngAfterViewInit(): void {
    const dialog = this.element.nativeElement;
    const heading = dialog.querySelector('h2,h3');
    if (heading) {
      heading.id ||= 'master-dialog-' + MasterDataModalDirective.nextId++;
      dialog.setAttribute('aria-labelledby', heading.id);
    }
    dialog.removeAttribute('open');
    dialog.showModal();
  }
  @HostListener('cancel', ['$event']) cancel(event: Event): void {
    event.preventDefault();
    if (!this.element.nativeElement.closest('fieldset[disabled]')) this.appModalClosed.emit();
  }
  @HostListener('click', ['$event']) backdrop(event: MouseEvent): void {
    if (event.target !== this.element.nativeElement || this.element.nativeElement.closest('fieldset[disabled]')) return;
    const box = this.element.nativeElement.getBoundingClientRect();
    if (event.clientX < box.left || event.clientX > box.right || event.clientY < box.top || event.clientY > box.bottom)
      this.appModalClosed.emit();
  }
  ngOnDestroy(): void {
    this.element.nativeElement.close();
    if (this.previousFocus?.isConnected) this.previousFocus.focus();
  }
}

@NgModule({
  imports: [KeyboardRowDirective, MasterDataModalDirective, FormFieldErrorComponent, PageStateComponent],
  exports: [KeyboardRowDirective, MasterDataModalDirective, FormFieldErrorComponent, PageStateComponent]
})
export class MasterDataUiModule {}
