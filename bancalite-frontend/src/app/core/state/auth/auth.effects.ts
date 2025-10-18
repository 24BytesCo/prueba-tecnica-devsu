import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { AuthActions } from './auth.actions';
import { AuthService } from '../../services/auth.service';
import { catchError, map, of, switchMap, tap } from 'rxjs';
import { Router } from '@angular/router';

@Injectable()
export class AuthEffects {
  login$;
  loginSuccessNavigate$;
  logout$;
  fetchMe$;

  constructor(private actions$: Actions, private auth: AuthService, private router: Router) {
    // Efecto de login: escucha la acción Login, llama al servicio y emite Success/Failure
    this.login$ = createEffect(() =>
      this.actions$.pipe(
        ofType(AuthActions.login),
        switchMap(({ email, password }) =>
          this.auth.login({ email, password }).pipe(
            tap(profile => {
              if (profile.token) localStorage.setItem('auth_token', profile.token);
              if (profile.refreshToken) localStorage.setItem('refresh_token', profile.refreshToken);
            }),
            map(profile => AuthActions.loginSuccess({ profile })),
            catchError(error => of(AuthActions.loginFailure({ error })))
          )
        )
      )
    );

    // Navega al dashboard tras loguearse correctamente
    this.loginSuccessNavigate$ = createEffect(
      () =>
        this.actions$.pipe(
          ofType(AuthActions.loginSuccess),
          tap(() => this.router.navigateByUrl('/'))
        ),
      { dispatch: false }
    );

    // Tras login success, solicitar /auth/me para obtener codeRol (Admin/User)
    this.fetchMe$ = createEffect(() =>
      this.actions$.pipe(
        ofType(AuthActions.loginSuccess),
        switchMap(() =>
          this.auth.me().pipe(
            map(profile => AuthActions.meSuccess({ profile })),
            // Si /me falla o devuelve nulos, continuamos sin romper el flujo
            catchError(() => of(AuthActions.meSuccess({ profile: { codeRol: null } as any })))
          )
        )
      )
    );

    // Al hacer logout limpiamos tokens y enviamos al login
    this.logout$ = createEffect(
      () =>
        this.actions$.pipe(
          ofType(AuthActions.logout),
          tap(() => {
            localStorage.removeItem('auth_token');
            localStorage.removeItem('refresh_token');
            this.router.navigateByUrl('/login');
          })
        ),
      { dispatch: false }
    );
  }
}
