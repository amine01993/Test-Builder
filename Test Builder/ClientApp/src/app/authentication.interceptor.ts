import { HttpErrorResponse, HttpEvent, HttpHandler, HttpHeaders, HttpInterceptor, HttpRequest, HttpStatusCode } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { Observable, throwError } from "rxjs";
import { catchError, map } from "rxjs/operators";
import { AuthenticationService } from "./authentication.service";
import { ModalService } from "./modal.service";

@Injectable()
export class AuthenticationInteceptor implements HttpInterceptor {

  constructor(private router: Router, private authService: AuthenticationService, private modalService: ModalService) { }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (req.params.has('auth') && req.params.get('auth')?.toString() === 'true') {
      const token = this.authService.getToken();
      const editedReq = req.clone({ headers: new HttpHeaders().set('Authorization', 'Bearer ' + token) });
      const editedReqObservable = next.handle(editedReq);
      return editedReqObservable.pipe(catchError((error: HttpErrorResponse) => {
        if (error.status === HttpStatusCode.Unauthorized) {
          //console.log('error response', error);
          const modalRef = this.modalService.open('login');
          modalRef?.result.then(result => {
            if (result === 1) {
              window.location.reload();
            }
          });
          this.router.navigate([], { queryParams: { modal: 'login' } });
        }
        return throwError(error);
      }));
    }

    return next.handle(req);
  }

}
