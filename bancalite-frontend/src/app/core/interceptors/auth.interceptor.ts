import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Tomamos el token desde localStorage (suficiente para este ejercicio).
    // En una versión más robusta lo sacaríamos de un store o un servicio seguro.
    const token = localStorage.getItem('auth_token');
    let authReq = req;
    // Si hay token agregamos el header Authorization: Bearer ...
    if (token) {
      authReq = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
    }
    return next.handle(authReq);
  }
}
