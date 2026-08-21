import { Injectable, inject, signal } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';

export type SupportedLanguage = 'be' | 'en';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly transloco = inject(TranslocoService);
  readonly current = signal<SupportedLanguage>(this.initialLanguage());

  constructor() { this.transloco.setActiveLang(this.current()); }

  set(language: string): void {
    const supported: SupportedLanguage = language === 'en' ? 'en' : 'be';
    this.current.set(supported);
    localStorage.setItem('leanprod.language', supported);
    document.documentElement.lang = supported;
    this.transloco.setActiveLang(supported);
  }

  private initialLanguage(): SupportedLanguage {
    const saved = localStorage.getItem('leanprod.language');
    if (saved === 'be' || saved === 'en') return saved;
    return navigator.language.toLowerCase().startsWith('en') ? 'en' : 'be';
  }
}
