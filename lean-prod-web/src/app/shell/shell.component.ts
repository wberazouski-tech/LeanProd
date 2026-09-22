import { Component, ViewChild, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { Permissions } from '../core/auth/permissions';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
@Component({ selector: 'app-shell', standalone: true, imports: [RouterOutlet, RouterLink, RouterLinkActive, TranslocoPipe], templateUrl: './shell.component.html', styleUrl: './shell.component.css' })
export class ShellComponent {
  @ViewChild(RouterOutlet) private outlet?: RouterOutlet;
  openEquipment(event: MouseEvent): void {
    if (event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
    if (this.router.url.split(/[?#]/)[0] !== '/master-data/equipment') return;
    event.preventDefault();
    const editor = this.outlet?.component as { closeEditor?: () => Promise<void> } | undefined;
    void editor?.closeEditor?.();
  }
  openItems(event: MouseEvent): void {
    if (event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
    if (this.router.url.split(/[?#]/)[0] !== '/catalog/products') return;
    event.preventDefault();
    const editor = this.outlet?.component as { closeEditor?: () => Promise<void> } | undefined;
    void editor?.closeEditor?.();
  }
  openTechnologies(event: MouseEvent): void {
    if (event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
    if (this.router.url.split(/[?#]/)[0] !== '/master-data/technologies') return;
    event.preventDefault();
    const editor = this.outlet?.component as { closeEditor?: () => Promise<void> } | undefined;
    void editor?.closeEditor?.();
  }
 readonly auth = inject(AuthService); readonly permissions = Permissions; private readonly router = inject(Router); private readonly translate = inject(TranslocoService); menuOpen = false; sidebarCollapsed = false; roleList(roles: string[]): string { return roles.map(role => this.translate.translate(`roles.${role}`)).join(', '); } toggleMenu(): void { this.menuOpen = !this.menuOpen; } toggleSidebar(): void { this.sidebarCollapsed = !this.sidebarCollapsed; } closeMenu(): void { this.menuOpen = false; } logout(): void { this.auth.logout().subscribe(() => void this.router.navigateByUrl('/login')); } }
