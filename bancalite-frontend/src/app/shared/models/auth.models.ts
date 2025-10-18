export interface LoginRequest {
  email: string;
  password: string;
}

export interface Profile {
  nombreCompleto?: string | null;
  email?: string | null;
  codeRol?: string | null; // 'Admin' | 'User'
  token?: string | null;
  refreshToken?: string | null;
  // Estado del cliente vinculado: true=activo, false=inactivo (null si no aplica)
  clienteActivo?: boolean | null;
}
