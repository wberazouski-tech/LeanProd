import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { LoadingService } from './loading.service';

export const loadingInterceptor: HttpInterceptorFn = (request, next) => {
  if (!/\/api\//i.test(request.url)) return next(request);

  const loading = inject(LoadingService);
  loading.begin();
  return next(request).pipe(finalize(() => loading.end()));
};
