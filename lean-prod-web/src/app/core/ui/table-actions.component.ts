import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';

@Component({
  selector: 'app-table-actions',
  standalone: true,
  imports: [TranslocoPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `@if (visible) { <div class="table-actions" role="toolbar" [attr.aria-label]="'common.actions' | transloco"><button type="button" class="minimal-button" [disabled]="!selected" (click)="editRequested.emit()">{{ 'common.edit' | transloco }}</button><button type="button" class="minimal-button" [disabled]="!selected" (click)="copyRequested.emit()">{{ 'common.copy' | transloco }}</button></div> }`,
  styles: [`:host{display:inline-flex}.table-actions{display:inline-flex;gap:.5rem;margin:0}.minimal-button{background:#fff;border:1px solid #b9c2cc;border-radius:6px;color:#344054;font-size:.82rem;line-height:1.2;padding:.28rem .6rem}.minimal-button:hover:not(:disabled){background:#eef2f5}.minimal-button:disabled{opacity:.42}`]
})
export class TableActionsComponent {
  @Input() visible = true;
  @Input() selected = false;
  @Output() readonly editRequested = new EventEmitter<void>();
  @Output() readonly copyRequested = new EventEmitter<void>();
}
