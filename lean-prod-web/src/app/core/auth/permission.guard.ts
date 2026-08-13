import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from './auth.service';
import { Permission } from './permissions';

export const permissionGuard = (permission: Permission): CanActivateFn => () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const decide = () => auth.hasPermission(permission)
    ? true
    : router.createUrlTree(['/forbidden']);

  return auth.currentUser()
    ? decide()
    : auth.restoreSession().pipe(map(() => decide()));
};
