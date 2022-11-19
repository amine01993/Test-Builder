import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { AuthenticationService } from '../authentication.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  login: FormGroup = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required])
  });

  submitting: boolean = false;
  serverErrors: { [key: string]: string } = {};

  get email(): FormControl {
    return this.login.get('email') as FormControl;
  }
  get password(): FormControl {
    return this.login.get('password') as FormControl;
  }

  constructor(
    public activeModal: NgbActiveModal,
    private httpClient: HttpClient,
    private authService: AuthenticationService,
  ) { }

  ngOnInit(): void {
    console.log('LoginComponent OnInit');
    //this.activeModal.
  }
  
  RemoveError(key: string): void {
    if (this.serverErrors.hasOwnProperty(key)) {
      delete this.serverErrors[key];
    }
  }

  OnSubmit(): void {
    console.log(this.login);
    if (this.login.valid) {
      this.submitting = true;
      this.httpClient.post<any>('api/token', {
        Email: this.login.value.email,
        Password: this.login.value.password,
      }).subscribe({
        next: (data: any) => {
          console.log('api/token - data', data);
          if (data.result) {
            this.authService.setToken(data.token);
            this.authService.updateHeader();
            this.serverErrors = {};
            this.activeModal.close(1);
          }
          else {
            this.serverErrors = data.errors;
          }
        },
        error: (error) => {
          console.log('api/token - error', error)
        },
        complete: () => { this.submitting = false; }
      });
    }
  }
}
