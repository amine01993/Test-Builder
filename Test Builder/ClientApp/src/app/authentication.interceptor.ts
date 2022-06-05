import { HttpEvent, HttpHandler, HttpHeaders, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { AuthenticationService } from "./authentication.service";

@Injectable()
export class AuthenticationInteceptor implements HttpInterceptor {

  constructor(private authService: AuthenticationService) { }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    
    if (req.params.has('auth') && req.params.get('auth')?.toString() === 'true') {
      var token = this.authService.getToken();
      var editedReq = req.clone({ headers: new HttpHeaders().set('Authorization', 'Bearer ' + token) });
      return next.handle(editedReq);
    }

    return next.handle(req);
  }

}
