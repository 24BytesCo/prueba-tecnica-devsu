export interface LoginRequest {
  email: string;
  password: string;
}

export interface Profile {
  nombreCompleto?: string | null;
  email?: string | null;
  token?: string | null;
  refreshToken?: string | null;
}

