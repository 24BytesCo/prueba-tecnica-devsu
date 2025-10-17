import { createFeature, createReducer, createSelector, on } from '@ngrx/store';
import { AuthActions } from './auth.actions';
import { Profile } from '../../../shared/models/auth.models';

export interface AuthState {
  loading: boolean;
  error: string | null;
  profile: Profile | null;
}

const initialState: AuthState = {
  loading: false,
  error: null,
  profile: null
};

// Reducer de autenticación: describe cómo cambian los estados ante cada acción
const reducer = createReducer(
  initialState,
  on(AuthActions.login, state => ({ ...state, loading: true, error: null })),
  on(AuthActions.loginSuccess, (state, { profile }) => ({ ...state, loading: false, profile })),
  on(AuthActions.loginFailure, (state, { error }) => ({ ...state, loading: false, error })),
  on(AuthActions.logout, state => ({ ...state, profile: null }))
);

// Feature con selectores derivados (NgRx 18 exige shape estricto / extraSelectors)
export const authFeature = createFeature({
  name: 'auth',
  reducer,
  extraSelectors: ({ selectAuthState }) => ({
    selectLoading: createSelector(selectAuthState, s => s.loading),
    selectProfile: createSelector(selectAuthState, s => s.profile),
    selectError: createSelector(selectAuthState, s => s.error)
  })
});

export const { name: authFeatureKey, reducer: authReducer, selectAuthState, selectLoading, selectProfile } = authFeature;
