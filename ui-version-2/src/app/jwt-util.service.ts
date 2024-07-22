import { Injectable } from '@angular/core';
// import jwt_decode from 'jwt-decode'; 
@Injectable({
  providedIn: 'root'
})
export class JwtUtilService {
  decodeToken(token: string): any {
    try {
      return JSON.parse(atob(token.split('.')[1]));
    } catch (err) {
      return null;
    }
  }
}
