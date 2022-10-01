import { HttpClient, HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ConfirmationModalComponent } from '../../confirmation-modal/confirmation-modal.component';
import { Question } from '../../question-add/question.model';
import { Test } from '../../test/test.model';

@Component({
  selector: 'app-question-item',
  templateUrl: './question-item.component.html',
  styleUrls: ['./question-item.component.scss']
})
export class QuestionItemComponent implements OnInit {

  @Input() question!: Question;
  @Input() key!: number;
  @Output() reloadDataEvent = new EventEmitter<void>();

  @Input() duplicating: boolean = false;
  @Input() deleting: boolean = false;
  @Input() usedInLoading: boolean = false;

  //@ViewChild(NgbPopover) popover!: NgbPopover;

  tests: Test[] = [];

  constructor(
    private httpClient: HttpClient,
    private router: Router,
    private modal: NgbModal,
  ) { }

  ngOnInit(): void {
    console.log(this.question);
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
    this.duplicating = true;

    this.httpClient.post<{ questionId: number }>('api/question/duplicate/' + question.Id, {}, { params: { auth: true } })
      .subscribe({
        next: data => {
          // navigate to edit page of the new question
          const editRoute = this.getEditRoute({ ...question, ...{ Id: data.questionId } });
          const url = this.router.serializeUrl(this.router.createUrlTree(editRoute));
          console.log('duplicate edit url', url);
          window.open(url, '_blank');
          this.duplicating = false;
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === HttpStatusCode.NotFound) {
            // display error message
          }
          this.duplicating = false;
        }
    });
  }

  OnDeleteQuestion(question: Question): void {
    const modalRef = this.modal.open(ConfirmationModalComponent);

    modalRef.componentInstance.title = "Deleting Question";
    modalRef.componentInstance.content = "Are you sure you want to delete this question ?";

    modalRef.result.then(result => {
      console.log('result', result);
      if (result === 1) {
        this.deleting = true;
        this.httpClient.delete<any>('api/question/delete/' + question.Id, { params: { auth: true } }).subscribe({
          next: data => {
            // reload list
            this.reloadDataEvent.emit();
            this.deleting = false;
          },
          error: (error: HttpErrorResponse) => {
            if (error.status === HttpStatusCode.Conflict) {
              // display error message
            }
            this.deleting = false;
          }
        });
      }
    }).catch(err => {
      console.log('result - err', err);
    });
  }

  OnUsedIn(question: Question, p: any): void {
    //console.log(this.popover);

    this.usedInLoading = true;
    this.httpClient.get<Test[]>('api/question/used-in/' + question.Id, { params: { auth: true } }).subscribe({
      next: tests => {
        console.log(tests);
        this.tests = tests;
        this.usedInLoading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.usedInLoading = false;
      }
    });
  }
}
