import { Component, Input } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';

export type PageState = 'loading' | 'empty' | 'error' | 'saving' | 'success' | 'ready';

@Component({
  selector: 'app-page-state',
  standalone: true,
  imports: [TranslocoPipe],
  template: `@if (state !== 'ready' && state !== 'empty') { <p class="page-state" [class.page-state-error]="state === 'error'" [class.page-state-success]="state === 'success'" role="status" aria-live="polite">{{ ('pageState.' + state) | transloco }}</p> }`
})
export class PageStateComponent {
  @Input() state: PageState = 'ready';
}