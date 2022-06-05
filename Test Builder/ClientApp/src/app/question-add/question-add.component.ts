import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { QuestionType } from './question.model';
import { QuestionService } from './question.service';

@Component({
  selector: 'app-question-add',
  templateUrl: './question-add.component.html',
  styleUrls: ['./question-add.component.css']
})
export class QuestionAddComponent implements OnInit, OnDestroy {

  questionTypes: QuestionType[] = [];
  questionTypeSub: Subscription | undefined;

  constructor(
    private questionService: QuestionService
  ) { }

  ngOnInit(): void {
    this.questionTypeSub = this.questionService.questionTypes.subscribe({
      next: (questionTypes) => {
        this.questionTypes = questionTypes;
      }
    });
    this.questionService.LoadQuestionTypes();
  }

  ngOnDestroy(): void {
    if (this.questionTypeSub)
      this.questionTypeSub.unsubscribe();
  }

}
