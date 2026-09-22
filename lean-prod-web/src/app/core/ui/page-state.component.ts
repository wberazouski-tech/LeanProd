import { ChangeDetectionStrategy, Component, EventEmitter, Output, input } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
export type PageState = 'loading' | 'empty' | 'error' | 'saving' | 'success' | 'ready';
@Component({
  selector: 'app-page-state', standalone: true, imports: [TranslocoPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `@if (state() !== 'ready') {
    <p class="page-state" [class.page-state-error]="state() === 'error'" [class.page-state-success]="state() === 'success'" role="status" aria-live="polite">{{ (state() === 'empty' ? 'common.noResults' : 'pageState.' + state()) | transloco }}</p>
    @if (state() === 'error' && retry.observed) { <button type="button" (click)="retry.emit()">{{ 'masterDataReview.retry' | transloco }}</button> }
  }`
})
export class PageStateComponent {
  @Output() readonly retry = new EventEmitter<void>();
  readonly state = input<PageState>('ready');
}
