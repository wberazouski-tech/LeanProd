import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const isAuthOperation = /\/identity\/(login|register|refresh|logout)$/.test(request.url);
  const withCredentials = request.clone({
    withCredentials: true,
    setHeaders: auth.accessToken && !isAuthOperation
      ? { Authorization: `Bearer ${auth.accessToken}` } : {}
  });

  return next(withCredentials).pipe(catchError((error: HttpErrorResponse) => {
    if (error.status !== 401 || isAuthOperation) return throwError(() => error);
    return auth.refresh().pipe(switchMap(() => next(request.clone({
      withCredentials: true,
      setHeaders: { Authorization: `Bearer ${auth.accessToken}` }
    }))));
  }));
};
