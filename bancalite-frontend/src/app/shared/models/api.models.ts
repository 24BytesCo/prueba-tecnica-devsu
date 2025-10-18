// Envoltorio estandarizado que devuelve nuestro backend
export interface ApiResult<T> {
  isSuccess: boolean;
  datos: T;
  error?: string | null;
}

