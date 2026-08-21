import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';

@Component({ selector: 'app-forbidden', standalone: true, imports: [RouterLink, TranslocoPipe], template: `<h1>{{ 'auth.forbiddenTitle' | transloco }}</h1><p>{{ 'auth.forbiddenText' | transloco }}</p><a routerLink="/">{{ 'auth.home' | transloco }}</a>` })
export class ForbiddenComponent {}
