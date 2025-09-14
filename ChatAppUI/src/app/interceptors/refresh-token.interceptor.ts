import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthenticationService } from '../services/authentication.service';
import { shouldSkipAuthentication } from './non-authenticated-endpoints';

export const refreshTokenInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthenticationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        const token = authService.getAccessToken();

        if (shouldSkipAuthentication(req.url) || !token) {
          return throwError(() => error);
        }
        return authService.refreshToken().pipe(
          switchMap(() => {
            const newToken = authService.getAccessToken();

            if (newToken) {
              const retryReq = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${newToken}`,
                },
              });
              return next(retryReq);
            } else {
              authService.clearTokens();
              return throwError(() => error);
            }
          }),
          catchError((refreshError) => {
            // If refresh fails, clear tokens
            authService.clearTokens();
            // You might want to redirect to login page here
            // const router = inject(Router);
            // router.navigate(['/login']);
            return throwError(() => refreshError);
          })
        );
      }

      // For other errors, just pass them through
      return throwError(() => error);
    })
  );
};
