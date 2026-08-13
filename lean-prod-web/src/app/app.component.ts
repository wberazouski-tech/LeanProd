import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ErrorBannerComponent } from './core/errors/error-banner.component';

@Component({ selector: 'app-root', standalone: true, templateUrl: './app.component.html',
  styleUrl: './app.component.css', imports: [RouterOutlet, ErrorBannerComponent] })
export class AppComponent {}
