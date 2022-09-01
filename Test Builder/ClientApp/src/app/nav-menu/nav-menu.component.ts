import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Subscription } from 'rxjs';
import { AuthenticationData } from '../authentication.model';
import { AuthenticationService } from '../authentication.service';
import { LoginComponent } from '../login/login.component';
import { RegisterComponent } from '../register/register.component';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss']
})
export class NavMenuComponent implements OnInit, OnDestroy {
  isExpanded = false;

  data: AuthenticationData = { name: '', loggedIn: false };
  authSub: Subscription | undefined;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private modal: NgbModal,
    private authService: AuthenticationService
  ) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe({
      next: queryParams => {
        if (queryParams.hasOwnProperty('modal')) {
          const modal = queryParams.modal;
          this.showModal(modal);
        }
        //console.log(queryParams)
      }
    });

    this.authSub = this.authService.subject.subscribe({
      next: (obj) => {
        this.data = obj;
      }
    });

    this.authService.updateHeader();
  }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  showModal(modal: string): void {
    switch (modal) {
      case 'login':
        this.modal.open(LoginComponent);
        break;
      case 'register':
        this.modal.open(RegisterComponent);
        break;
    }
  }

  Logout() {
    this.data.loggedIn = false;
    this.authService.removeToken();
    this.router.navigate(['/']);
  }

  ngOnDestroy(): void {
    if (this.authSub)
      this.authSub.unsubscribe();
  }
}
