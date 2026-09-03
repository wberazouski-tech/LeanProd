import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ErrorBannerComponent } from './core/errors/error-banner.component';
import { AuthService } from './core/auth/auth.service';
import { LanguageService, SupportedLanguage } from './core/i18n/language.service';
import { TranslocoPipe } from '@jsverse/transloco';
import { LoadingService } from './core/loading/loading.service';

@Component({ selector: 'app-root', standalone: true, templateUrl: './app.component.html',
  styleUrl: './app.component.css', imports: [RouterOutlet, ErrorBannerComponent, TranslocoPipe] })
export class AppComponent {
  private readonly auth = inject(AuthService);
  readonly language = inject(LanguageService);
  readonly loading = inject(LoadingService);

  changeLanguage(value: string): void {
    const language = value as SupportedLanguage;
    if (this.auth.currentUser()) this.auth.updateLanguage(language).subscribe();
    else this.language.set(language);
  }
}
