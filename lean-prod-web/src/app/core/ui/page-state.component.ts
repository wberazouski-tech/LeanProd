import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';

export type PageState = 'loading' | 'empty' | 'error' | 'saving' | 'success' | 'ready';

@Component({
  selector: 'app-page-state',
  standalone: true,
  imports: [TranslocoPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `@if (state() !== 'ready' && state() !== 'empty' && state() !== 'loading') { <p class="page-state" [class.page-state-error]="state() === 'error'" [class.page-state-success]="state() === 'success'" role="status" aria-live="polite">{{ ('pageState.' + state()) | transloco }}</p> }`
})
export class PageStateComponent {
  readonly state = input<PageState>('ready');
}
