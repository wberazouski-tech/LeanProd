import { Injectable, signal } from '@angular/core';
@Injectable({ providedIn: 'root' })
export class ErrorService { readonly message = signal<string | null>(null); show(message: string): void { this.message.set(message); } clear(): void { this.message.set(null); } }
