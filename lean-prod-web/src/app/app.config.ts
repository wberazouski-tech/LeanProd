import { ApplicationConfig } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { authInterceptor } from './core/auth/auth.interceptor';
import { errorInterceptor } from './core/errors/error.interceptor';
import { provideTransloco } from '@jsverse/transloco';
import { TranslocoHttpLoader } from './core/i18n/transloco-http-loader';
import { loadingInterceptor } from './core/loading/loading.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([loadingInterceptor, errorInterceptor, authInterceptor])),
    provideTransloco({ config: { availableLangs: ['be', 'en'], defaultLang: 'be', fallbackLang: 'be', reRenderOnLangChange: true }, loader: TranslocoHttpLoader })
  ]
};
