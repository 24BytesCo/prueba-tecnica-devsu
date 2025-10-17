import { createActionGroup, props, emptyProps } from '@ngrx/store';
import { Profile } from '../../../shared/models/auth.models';

// Agrupamos todas las acciones de autenticación en un único namespace "Auth"
export const AuthActions = createActionGroup({
  source: 'Auth',
  events: {
    // Disparada al enviar el formulario de login
    'Login': props<{ email: string; password: string }>(),
    // Éxito del login, el backend devuelve el Profile (con token y refresh)
    'Login Success': props<{ profile: Profile }>(),
    // Error del login (ProblemDetails u otro payload)
    'Login Failure': props<{ error: any }>(),
    // Cerrar sesión de forma explícita
    'Logout': emptyProps()
  }
});
