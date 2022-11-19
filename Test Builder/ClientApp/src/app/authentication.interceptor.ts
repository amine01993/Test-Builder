import { HttpErrorResponse, HttpEvent, HttpHandler, HttpHeaders, HttpInterceptor, HttpRequest, HttpResponse, HttpStatusCode } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { Observable, throwError } from "rxjs";
import { catchError, map } from "rxjs/operators";
import { AuthenticationService } from "./authentication.service";
import { LoginComponent } from "./login/login.component";

@Injectable()
export class AuthenticationInteceptor implements HttpInterceptor {

  constructor(private modal: NgbModal, private authService: AuthenticationService) { }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    
    if (req.params.has('auth') && req.params.get('auth')?.toString() === 'true') {
      const token = this.authService.getToken();
      const editedReq = req.clone({ headers: new HttpHeaders().set('Authorization', 'Bearer ' + token) });
      const editedReqObservable = next.handle(editedReq);
      return editedReqObservable.pipe(catchError((error: HttpErrorResponse) => {
        if (error.status === HttpStatusCode.Unauthorized && !this.authService.authModalOpen) {
          //console.log('error response', error);
          this.authService.authModalOpen = true;
          const modalRef = this.modal.open(LoginComponent);
          modalRef.result.then(result => {
            console.log('modal result', result);
            if (result === 1) {
              window.location.reload();
            }
            this.authService.authModalOpen = false;
          })
            .catch(reason => {
              this.authService.authModalOpen = false;
            });
        }
        return throwError(error);
      }));
    }

    return next.handle(req);
  }

}
