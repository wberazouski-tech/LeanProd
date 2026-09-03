import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';

@Component({ selector: 'app-modal-shell', standalone: true, imports: [TranslocoPipe], template: `@if (open) { <div class="modal-layer" (click)="closed.emit()"><dialog open [class]="dialogClass" [attr.aria-labelledby]="titleId"><section class="editor" (click)="$event.stopPropagation()"><header class="modal-header"><h2 [id]="titleId">{{ titleKey | transloco }}</h2><button type="button" class="icon-button" (click)="closed.emit()" [attr.aria-label]="'common.close' | transloco">&times;</button></header><ng-content /></section></dialog></div> }`, styleUrl: './modal-shell.component.css' })
export class ModalShellComponent { @Input() open = false; @Input({ required: true }) titleKey = ''; @Input() dialogClass = 'department-dialog'; @Output() readonly closed = new EventEmitter<void>(); readonly titleId = `modal-${Math.random().toString(36).slice(2)}`; }
