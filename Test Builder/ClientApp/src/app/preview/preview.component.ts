import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { interval, Subscription } from 'rxjs';
import { take } from 'rxjs/operators';
import { Page, Test } from '../test/test.model';

@Component({
  selector: 'app-preview',
  templateUrl: './preview.component.html',
  styleUrls: ['./preview.component.scss']
})
export class PreviewComponent implements OnInit {

  test: Test | undefined;
  currentPageIndex: number = 0;
  progress: number = 0;
  answered: number[] = [];
  totalAnswered: number = 0;
  totalQuestions: number = 0;
  remainingTime: number | null = 0;
  intervalSub: Subscription | undefined;
  showTestSummary: boolean = false;
  summary = {
    correct: 0,
    unCorrect: 0,
    unAnswered: 0,
  };

  private testAnswers: {[key: number]: any[]} = {};

  constructor(
    private httpClient: HttpClient,
    private route: ActivatedRoute,
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe({
      next: params => {
        const testId = +params['id'];
        console.log('testId', params);

        this.httpClient.get<Test>('api/preview/' + testId, { params: { auth: true } }).subscribe({
          next: test => {
            console.log('Test', test);
            this.test = test;
            this.answered = Array(test.Pages.length).fill(0);
            this.totalQuestions = test.Pages.map(p => p.PageQuestions.length).reduce((pv, cv) => pv + cv, 0);
            this.SetPage(test.Pages[this.currentPageIndex]);
          }
        });
      }
    });
  }

  SetPage(page: Page) {
    this.remainingTime = page.Limit;
    if (this.remainingTime) {
      this.intervalSub = interval(1000).pipe(take(this.remainingTime)).subscribe({
        next: x => {
          //console.log(page.Name, x);
          this.remainingTime!--;
          if (this.remainingTime === 0) {
            this.OnNext();
          }
        }
      })
    }
  }

  OnPrevious() {
    console.log('OnPrevious');
  }

  OnNext() {
    if (this.currentPageIndex < this.test!.Pages.length - 1) {
      this.currentPageIndex++;
      if (this.intervalSub) {
        this.intervalSub.unsubscribe();
      }
      this.SetPage(this.test!.Pages[this.currentPageIndex]);
    }
  }

  UpdateAnswers({ questionId, answers }: { questionId: number, answers: any[]}) {
    this.testAnswers[questionId] = answers;
    // set answered
    let answered = 0;
    for (const pq of this.test?.Pages[this.currentPageIndex].PageQuestions!) {
      if (this.testAnswers.hasOwnProperty(pq.QuestionId!) && this.testAnswers[pq.QuestionId!].length > 0) {
        answered++;
      }
    }
    this.answered[this.currentPageIndex] = answered;
    this.totalAnswered = this.answered.reduce((pv, cv) => pv + cv);
    this.progress = this.totalAnswered / this.totalQuestions * 100;
  }

  OnSubmit() {
    if (this.intervalSub) {
      this.intervalSub.unsubscribe();
    }
    for (const page of this.test?.Pages!) {
      for (const pq of page.PageQuestions) {
        if (this.testAnswers.hasOwnProperty(pq.QuestionId!) && this.testAnswers[pq.QuestionId!].length > 0) {

          const unCorrectAnswers = pq.Question?.Answers!.filter(answer => !answer.Correct && this.testAnswers[pq.QuestionId!].includes(answer.Id + ''));
          if (unCorrectAnswers && unCorrectAnswers?.length > 0) {
            this.summary.unCorrect++;
          }
          else {
            this.summary.correct++;
          }
        }
        else {
          this.summary.unAnswered++;
        }
      }
    }
     this.showTestSummary = true;
  }

}
