import { ActionReducer, INIT, UPDATE } from '@ngrx/store';

// Simple hydration meta-reducer that persists the whole root state
// into localStorage and rehydrates it on app init/update.
const STORAGE_KEY = 'app_state';

export function hydrationMetaReducer<State>(reducer: ActionReducer<State>): ActionReducer<State> {
  return (state, action) => {
    if (action.type === INIT || action.type === UPDATE) {
      try {
        const storageValue = localStorage.getItem(STORAGE_KEY);
        if (storageValue) {
          const parsed = JSON.parse(storageValue) as State;
          return reducer(parsed, action);
        }
      } catch {
        // ignore hydration errors
      }
    }

    const nextState = reducer(state as State, action);
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(nextState));
    } catch {
      // ignore persistence errors (quota, etc.)
    }
    return nextState;
  };
}

