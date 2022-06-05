import { Component, OnDestroy, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'Test Builder';

  constructor(
  ) { }

  ngOnInit(): void {
    
  }

  ngOnDestroy(): void {
  }
}
