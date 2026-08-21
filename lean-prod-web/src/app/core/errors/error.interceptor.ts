import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { catchError, throwError } from 'rxjs';
import { ApiProblem } from './api-problem';
import { ErrorService } from './error.service';

export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const errors = inject(ErrorService);
  const translate = inject(TranslocoService);
  return next(request).pipe(catchError((response: HttpErrorResponse) => {
    const hasLocalAuthError = /\/api\/identity\/(login|register)$/i.test(request.url);
    if (hasLocalAuthError) return throwError(() => response);
    const problem = response.error as ApiProblem | undefined;
    const validation = problem?.errors ? Object.values(problem.errors).flat().join(' ') : '';
    const trace = problem?.traceId ? ` Trace ID: ${problem.traceId}` : '';
    errors.show(`${validation || problem?.detail || problem?.title || translate.translate('common.requestFailed')}${trace}`);
    return throwError(() => response);
  }));
};
