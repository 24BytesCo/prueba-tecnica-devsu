import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, Profile } from '../../shared/models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  // Base de la API: usamos environment para habilitar proxy / build
  private baseUrl = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  // Realiza login contra /api/auth/login y retorna el Profile del backend
  login(payload: LoginRequest): Observable<Profile> {
    return this.http.post<Profile>(`${this.baseUrl}/auth/login`, payload);
  }
}
