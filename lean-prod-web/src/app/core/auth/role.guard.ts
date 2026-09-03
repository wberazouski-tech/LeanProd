import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from './auth.service';

export const roleGuard = (role: string): CanActivateFn => () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const decide = () => auth.hasRole(role)
    ? true
    : router.createUrlTree(['/forbidden']);

  return auth.currentUser()
    ? decide()
    : auth.restoreSession().pipe(map(() => decide()));
};
