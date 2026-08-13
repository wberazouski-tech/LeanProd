import { Component, inject } from '@angular/core';
import { ErrorService } from './error.service';
@Component({ selector: 'app-error-banner', standalone: true, template: `@if (errors.message(); as message) { <div class="error-banner" role="alert"><span>{{ message }}</span><button type="button" (click)="errors.clear()" aria-label="Закрыць">×</button></div> }` })
export class ErrorBannerComponent { readonly errors = inject(ErrorService); }
