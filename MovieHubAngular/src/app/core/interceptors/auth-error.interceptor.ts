// core/interceptors/auth-error.interceptor.ts
import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

const AUTH_ENDPOINTS = ['/Usuarios/login', '/Usuarios/register'];

export const authErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const isAuthRequest = AUTH_ENDPOINTS.some((endpoint) => req.url.includes(endpoint));

      // Solo forzamos logout en 401 de peticiones YA autenticadas.
      // Si el 401 viene del propio /login (credenciales incorrectas), no debe desloguear ni redirigir.
      if (error.status === 401 && !isAuthRequest) {
        auth.logout();
      }

      return throwError(() => error);
    }),
  );
};
