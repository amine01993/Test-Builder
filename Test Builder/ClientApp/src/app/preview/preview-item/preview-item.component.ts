import { Component, Input, OnInit } from '@angular/core';
import { Question } from '../../question-add/question.model';
import { Test } from '../../test/test.model';

@Component({
  selector: 'app-preview-item',
  templateUrl: './preview-item.component.html',
  styleUrls: ['./preview-item.component.scss']
})
export class PreviewItemComponent implements OnInit {

  @Input() question!: Question;
  @Input() key!: number;

  @Input() duplicating: boolean = false;
  @Input() deleting: boolean = false;
  @Input() usedInLoading: boolean = false;

  tests: Test[] = [];

  constructor() { }

  ngOnInit(): void {
  }

  getEditRoute(question: Question): Array<string | number> {
    switch (question.TypeId) {
      case 1:
        return ['/admin', 'add-question', 0, 'multiple-choice', question.Id!];
      case 2:
        return ['/admin', 'add-question', 0, 'true-false', question.Id!];
      case 3:
        return ['/admin', 'add-question', 0, 'matching', question.Id!];
      case 4:
        return ['/admin', 'add-question', 0, 'free-text', question.Id!];
      case 5:
        return ['/admin', 'add-question', 0, 'essay', question.Id!];
    }
    return [];
  }

  OnDuplicateQuestion(question: Question): void {
  }

  OnDeleteQuestion(question: Question): void {
  }

  OnUsedIn(question: Question, p: any): void {
    //console.log(this.popover);
  }
}
