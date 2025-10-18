import { Injectable } from '@angular/core';
import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private router: Router) {}
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        // Nuestro backend estandariza errores en ProblemDetails { title, status, detail } (camelCase)
        // Map: tomamos title/detail directamente y usamos un fallback legible si faltan.
        const payload = error.error;
        let title = 'Error';
        let message = 'Ocurrió un error inesperado';

        if (payload && typeof payload === 'object') {
          title = (payload.title as string) ?? title;
          message = (payload.detail as string) ?? message;
        } else if (typeof payload === 'string') {
          try {
            const parsed = JSON.parse(payload);
            title = parsed?.title ?? title;
            message = parsed?.detail ?? message;
          } catch {
            // Si llegara texto plano por alguna ruta atípica, lo mostramos como mensaje
            message = payload;
          }
        }

        // Fallback por código si no vino título/mensaje (poco probable dada la estandarización)
        if (!message) {
          const map: Record<number, string> = {
            400: 'Solicitud inválida',
            401: 'No autorizado',
            403: 'Prohibido',
            404: 'No encontrado',
            409: 'Conflicto',
            422: 'Datos no procesables'
          };
          title = map[error.status] ?? title;
          message = map[error.status] ?? 'Ocurrió un error';
        }

        // 401: redirigir a login después de mostrar el modal
        if (error.status === 401) {
          localStorage.removeItem('auth_token');
          localStorage.removeItem('refresh_token');
          Swal.fire({ icon: 'error', title, text: message }).then(() => {
            this.router.navigateByUrl('/login');
          });
        } else {
          Swal.fire({ icon: 'error', title, text: message });
        }

        return throwError(() => error);
      })
    );
  }
}
