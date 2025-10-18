import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse } from '@angular/common/http';
import { Observable, finalize, tap } from 'rxjs';
import Swal from 'sweetalert2';
import { LoaderService } from '../services/loader.service';

@Injectable()
export class LoaderInterceptor implements HttpInterceptor {
  constructor(private loader: LoaderService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    this.loader.start();
    const method = (req.method || 'GET').toUpperCase();
    const shouldToastSuccess = method === 'POST' || method === 'PUT' || method === 'PATCH' || method === 'DELETE';

    let succeeded = false;
    return next.handle(req).pipe(
      tap({
        next: (event) => {
          if (event instanceof HttpResponse && event.ok) {
            succeeded = true;
          }
        },
        error: () => {
          succeeded = false;
        }
      }),
      finalize(() => {
        if (shouldToastSuccess && succeeded) {
          Swal.fire({
            toast: true,
            position: 'top-end',
            icon: 'success',
            title: 'Operación exitosa',
            showConfirmButton: false,
            timer: 1500,
            timerProgressBar: true
          });
        }
        this.loader.stop();
      })
    );
  }
}
