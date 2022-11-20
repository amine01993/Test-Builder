import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthenticationData } from '../authentication.model';
import { AuthenticationService } from '../authentication.service';
import { ModalService } from '../modal.service';

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
    private authService: AuthenticationService,
    private modalService: ModalService,
  ) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe({
      next: queryParams => {
        if (queryParams.hasOwnProperty('modal')) {
          const modal = queryParams.modal;
          this.modalService.open(modal);
        }
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


  Logout() {
    this.data.loggedIn = false;
    this.authService.removeToken();
    this.router.navigate([]);
  }

  ngOnDestroy(): void {
    if (this.authSub)
      this.authSub.unsubscribe();
  }
}
