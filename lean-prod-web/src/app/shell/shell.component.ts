import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { Permissions } from '../core/auth/permissions';
@Component({ selector: 'app-shell', standalone: true, imports: [RouterOutlet, RouterLink, RouterLinkActive], templateUrl: './shell.component.html', styleUrl: './shell.component.css' })
export class ShellComponent { readonly auth = inject(AuthService); readonly permissions = Permissions; private readonly router = inject(Router); logout(): void { this.auth.logout().subscribe(() => void this.router.navigateByUrl('/login')); } }
