import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import LayoutService from '../layout.service';
import { Test } from '../test/test.model';

@Component({
  selector: 'app-preview',
  templateUrl: './preview.component.html',
  styleUrls: ['./preview.component.scss']
})
export class PreviewComponent implements OnInit {

  test: Test | undefined;
  currentPageIndex: number = 0;
  progress: number = 0;
  answered: number = 0;
  totalQuestions: number = 0;
  remainingTime: number|null = 0;

  constructor(
    private layoutService: LayoutService,
    private httpClient: HttpClient,
    private route: ActivatedRoute,
  ) { }

  ngOnInit(): void {
    this.layoutService.setLayout('preview');

    this.route.params.subscribe({
      next: params => {
        const testId = +params['id'];

        this.httpClient.get<Test>('api/preview/' + testId).subscribe({
          next: test => {
            console.log('Test', test);
            this.test = test;
            this.totalQuestions = test.Pages.map(p => p.PageQuestions.length).reduce((pv, cv) => pv + cv, 0);
            this.remainingTime = test.Pages[this.currentPageIndex].Limit;
          }
        });
      }
    });
    
  }

}
