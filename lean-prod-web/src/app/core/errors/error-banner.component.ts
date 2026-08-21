import { Component, inject } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import { ErrorService } from './error.service';

@Component({ selector: 'app-error-banner', standalone: true, imports: [TranslocoPipe], template: `@if (errors.message(); as message) { <div class="error-banner" role="alert"><span>{{ message }}</span><button type="button" (click)="errors.clear()" [attr.aria-label]="'common.close' | transloco">×</button></div> }` })
export class ErrorBannerComponent { readonly errors = inject(ErrorService); }
