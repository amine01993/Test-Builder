import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Test } from './test.model';

@Component({
  selector: 'app-test',
  templateUrl: './test.component.html',
  styleUrls: ['./test.component.scss']
})
export class TestComponent implements OnInit {

  tests: Test[] = [];

  constructor(
    private httpClient: HttpClient,
  ) { }

  ngOnInit(): void {
    this.httpClient.get<Test[]>('api/test/list', { params: { auth: true } }).subscribe({
      next: tests => {
        console.log('tests list', tests);
        this.tests = tests;
      }
    })
  }

}
