import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Subject } from "rxjs";
import { AuthenticationData } from "./authentication.model";

@Injectable({
  providedIn: 'root'
})
export class AuthenticationService {

  subject: Subject<AuthenticationData> = new Subject();

  constructor(private httpClient: HttpClient) { }

  getToken(): string | null {
    var token = localStorage.getItem('token');
    return token;
  }

  setToken(token: string): void {
    localStorage.setItem('token', token);
  }

  removeToken(): void {
    localStorage.removeItem('token');
  }

  isAuth(): boolean {
    return localStorage.getItem('token') !== null;
  }

  updateHeader() {
    if (this.isAuth()) {
      this.subject.next({ loggedIn: true, name: '' });

      this.httpClient.get<AuthenticationData>('api/token/auth', {
        params: { auth: true }
      }).subscribe({
        next: (data) => {
          //console.log('updateHeader', data)
          this.subject.next({ ...data, ...{ loggedIn: true } });
        },
        error: (err) => {
          //console.error(err);
          if (err.status === 401) { // Unauthorized
            this.removeToken();
          }
          this.subject.next({ loggedIn: false, name: '' });
        }
      });
    }

  }
}
