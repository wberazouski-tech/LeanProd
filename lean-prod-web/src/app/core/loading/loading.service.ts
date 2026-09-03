import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private activeRequests = 0;
  private delayTimer?: ReturnType<typeof setTimeout>;
  private readonly visibleState = signal(false);

  readonly visible = this.visibleState.asReadonly();

  begin(): void {
    this.activeRequests += 1;
    if (this.activeRequests !== 1) return;
    this.delayTimer = setTimeout(() => {
      if (this.activeRequests > 0) this.visibleState.set(true);
    }, 1000);
  }

  end(): void {
    this.activeRequests = Math.max(0, this.activeRequests - 1);
    if (this.activeRequests > 0) return;
    if (this.delayTimer) clearTimeout(this.delayTimer);
    this.delayTimer = undefined;
    this.visibleState.set(false);
  }
}
