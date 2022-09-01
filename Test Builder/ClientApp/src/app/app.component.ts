import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import LayoutService from './layout.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'Test Builder';
  layout = 'admin';

  layoutSub: Subscription | undefined;

  constructor(
    private layoutSevice: LayoutService,
  ) { }

  ngOnInit(): void {
    this.layoutSub = this.layoutSevice.subject.subscribe({
      next: (type: string) => {
        this.layout = type;
      }
    });
  }

  ngOnDestroy(): void {
    if (this.layoutSub)
      this.layoutSub.unsubscribe();
  }
}
