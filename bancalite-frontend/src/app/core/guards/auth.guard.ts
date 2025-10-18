import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private router: Router) {}

  canActivate(): boolean | UrlTree {
    // Guard simple: si hay token damos paso, si no llevamos al login
    const token = localStorage.getItem('auth_token');
    if (token) return true;
    return this.router.parseUrl('/login');
  }
}
