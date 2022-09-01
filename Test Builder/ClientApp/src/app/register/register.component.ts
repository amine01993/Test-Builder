import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { AuthenticationService } from '../authentication.service';
import { passwordMatchValidator } from './register.validator';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {

  register: FormGroup = new FormGroup({
    name: new FormControl('', [Validators.required]),
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required, Validators.minLength(6)]),
    confirmPassword: new FormControl('', [Validators.required]),
  }, [passwordMatchValidator()]);

  serverErrors: { [key: string]: string } = {};

  get name(): FormControl {
    return this.register.get('name') as FormControl;
  }
  get email(): FormControl {
    return this.register.get('email') as FormControl;
  }
  get password(): FormControl {
    return this.register.get('password') as FormControl;
  }
  get confirmPassword(): FormControl {
    return this.register.get('confirmPassword') as FormControl;
  }

  constructor(
    private httpClient: HttpClient,
    public activeModal: NgbActiveModal,
    private authService: AuthenticationService
  ) { }

  ngOnInit(): void {
  }

  OnSubmit(): void {
    console.log(this.register);
    if (this.register.valid) {
      this.httpClient.post<any>('api/token/register', {
        Name: this.register.value.name,
        Email: this.register.value.email,
        Password: this.register.value.password,
      }).subscribe({
        next: (data: any) => {
          console.log('api/token/register - data', data);
          if (data.result) {
            this.authService.setToken(data.token);
            this.authService.updateHeader();
            this.serverErrors = {};
            this.activeModal.close();
          }
          else {
            this.serverErrors = data.errors;
          }
        },
        error: (error) => {
          console.log('api/token/register - error', error)
        }
      });
    }
  }

}
