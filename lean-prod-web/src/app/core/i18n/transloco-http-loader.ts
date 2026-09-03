import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Translation, TranslocoLoader } from '@jsverse/transloco';

@Injectable({ providedIn: 'root' })
export class TranslocoHttpLoader implements TranslocoLoader {
  private readonly http = inject(HttpClient);
  private readonly assetVersion = Date.now();
  getTranslation(language: string) {
    return this.http.get<Translation>(`assets/i18n/${language}.json?v=${this.assetVersion}`);
  }
}
