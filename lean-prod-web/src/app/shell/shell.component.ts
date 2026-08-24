import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { Permissions } from '../core/auth/permissions';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
@Component({ selector: 'app-shell', standalone: true, imports: [RouterOutlet, RouterLink, RouterLinkActive, TranslocoPipe], templateUrl: './shell.component.html', styleUrl: './shell.component.css' })
export class ShellComponent { readonly auth = inject(AuthService); readonly permissions = Permissions; private readonly router = inject(Router); private readonly translate = inject(TranslocoService); menuOpen = false; roleList(roles: string[]): string { return roles.map(role => this.translate.translate(`roles.${role}`)).join(', '); } toggleMenu(): void { this.menuOpen = !this.menuOpen; } closeMenu(): void { this.menuOpen = false; } logout(): void { this.auth.logout().subscribe(() => void this.router.navigateByUrl('/login')); } }
