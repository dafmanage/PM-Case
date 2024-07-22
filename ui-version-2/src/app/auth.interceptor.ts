import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req);
};
import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { TokenStorageService } from './core/services/token-storage.service';
import { Router } from '@angular/router';
import { JwtUtilService } from './jwt-util.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(
    private TokenStorageService: TokenStorageService,
    private router: Router,
    private jwtUtilService: JwtUtilService,
   ) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
     // Get the JWT token from the AuthService
     const token = this.TokenStorageService.getToken();

     // Clone the request and add the token to the headers
 
     if (token) {
      // Decode the JWT token to get the user information
      const userInfo = this.jwtUtilService.decodeToken(token);

      // Store the username and role in the session storage or secure storage
      sessionStorage.setItem('role', userInfo.role);
      // Clone the request and add the JWT token to the headers
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }
 
     return next.handle(request);
  }
}