import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { NgbModal, NgbModalRef } from "@ng-bootstrap/ng-bootstrap";
import { LoginComponent } from "./login/login.component";
import { RegisterComponent } from "./register/register.component";

@Injectable({
  providedIn: 'root'
})
export class ModalService {

  modals: Map<string, NgbModalRef> = new Map;

  constructor(private modal: NgbModal, private router: Router) { }

  open(modal: string): NgbModalRef | null {
    let ref: NgbModalRef | null = null;
    if (modal === 'login') {
      if (this.modals.has('login')) return null;
      else if (this.modals.has('register')) {
        this.modals.get('register')?.close(-1);
        this.modals.delete('register');
      }
      ref = this.modal.open(LoginComponent);
      this.modals.set('login', ref);
      ref.result.then(result => result === -1 ? this.modals.delete('login') : this.close('login'))
        .catch(error => this.close('login'));
    }
    else if (modal === 'register') {
      if (this.modals.has('register')) return null;
      else if (this.modals.has('login')) {
        this.modals.get('login')?.close(-1);
        this.modals.delete('login');
      }
      ref = this.modal.open(RegisterComponent);
      this.modals.set('register', ref);
      ref.result.then(result => result === -1 ? this.modals.delete('register') : this.close('register'))
        .catch(error => this.close('register'));
    }

    return ref;
  }

  close(modal: string) {
    this.modals.delete(modal);
    this.router.navigate([], { queryParams: { modal: '' } });
  }
}
