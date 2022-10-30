import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormArray, FormControl, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ConfirmationModalComponent } from '../../confirmation-modal/confirmation-modal.component';
import { PageSettingsComponent } from '../page-settings/page-settings.component';
import { Page, PageQuestion } from '../test.model';

@Component({
  selector: 'app-page-edit',
  templateUrl: './page-edit.component.html',
  styleUrls: ['./page-edit.component.scss']
})
export class PageEditComponent implements OnInit {

  page: Page | undefined;

  preview: FormControl = new FormControl(false);
  pageForm: FormGroup = new FormGroup({
    positions: new FormArray([])
  });

  constructor(
    private httpClient: HttpClient,
    private router: Router,
    private route: ActivatedRoute,
    private modal: NgbModal,
  ) { }

  get positions(): FormArray {
    return this.pageForm.get('positions') as FormArray;
  }

  ngOnInit(): void {
    this.route.params.subscribe({
      next: params => {
        const pageId = params['page-id'];
        console.log('pageId', pageId);
        this.httpClient.get<Page>('api/page/' + pageId, { params: { auth: true } })
          .subscribe({
            next: page => {
              this.page = page;
              console.log('this.page', this.page);
              //this.testQuestions = data.questions;
              for (const pq of this.page.PageQuestions) {
                this.positions.controls.push(new FormControl(pq.Position));
              }
            }
        });
      }
    });
  }

  ///**
  // * Moves an item in a FormArray to another position.
  // * @param formArray FormArray instance in which to move the item.
  // * @param fromIndex Starting index of the item.
  // * @param toIndex Index to which he item should be moved.
  // */
  //moveItemInFormArray(formArray: FormArray, fromIndex: number, toIndex: number): void {
  //  const from = this.clamp(fromIndex, formArray.length - 1);
  //  const to = this.clamp(toIndex, formArray.length - 1);

  //  if (from === to) {
  //    return;
  //  }

  //  const previous = formArray.at(from);
  //  const current = formArray.at(to);
  //  formArray.setControl(to, previous);
  //  formArray.setControl(from, current);
  //}

  ///** Clamps a number between zero and a maximum. */
  //clamp(value: number, max: number): number {
  //  return Math.max(0, Math.min(max, value));
  //}


  drop(event: CdkDragDrop<any>): void {
    if (event.previousIndex === event.currentIndex)
      return;

    moveItemInArray(this.page!.PageQuestions, event.previousIndex, event.currentIndex);

    let index = 0;
    const questionPositions = [];
    for (const pageQuestion of this.page!.PageQuestions) {
      pageQuestion.Position = index;
      index++;
      questionPositions.push({ Id: pageQuestion.Id, _Position: pageQuestion.Position });
    }
    //this.moveItemInFormArray(this.positions, event.previousIndex, event.currentIndex);

    this.httpClient.post<any>('api/page-question/positions', questionPositions, {
      params: { auth: true }
    }).subscribe({
      next: data => { }
    });

    console.log(this.page!.PageQuestions, this.positions.controls.map(c => c.value));
  }

  OnChangePosition(pageQuestion: PageQuestion, positionIndex: number): void {
    const positionControl = this.positions.at(positionIndex);
    console.log(pageQuestion, positionIndex, positionControl);
    const oldPosition = pageQuestion.Position, newPosition = +positionControl.value;
    const targetQuestion = this.page!.PageQuestions.find(pq => pq.Position === newPosition);
    const targetControl = this.positions.at(newPosition);

    pageQuestion.Position = newPosition;
    if (targetQuestion)
      targetQuestion.Position = oldPosition;
    targetControl.setValue(oldPosition);

    const tmp = this.page!.PageQuestions[oldPosition];
    this.page!.PageQuestions[oldPosition] = this.page!.PageQuestions[newPosition];
    this.page!.PageQuestions[newPosition] = tmp;

    const tmp1 = this.positions.at(oldPosition);
    this.positions.controls[oldPosition] = this.positions.at(newPosition);
    this.positions.controls[newPosition] = tmp1;

    const questionPositions = [
      { Id: pageQuestion.Id, _Position: pageQuestion.Position },
      { Id: targetQuestion?.Id, _Position: targetQuestion?.Position },
    ];

    this.httpClient.post<any>('api/page-question/positions', questionPositions, {
      params: { auth: true }
    }).subscribe({
      next: data => { }
    });
  }

  OnEditPageSettings(): void {
    const pageRef = this.modal.open(PageSettingsComponent, { size: 'lg' });

    pageRef.componentInstance.testId = this.page?.TestId;
    pageRef.componentInstance.page = this.page;

    pageRef.result
      .then((_page: Page) => {
        console.log('page', _page);

        this.page!.Name = _page.Name;
        this.page!.Shuffle = _page.Shuffle;
        this.page!.Limit = _page.Limit;
      })
      .catch(c => {
        console.log('catch', c);
      });
  }

  OnDeletePage(): void {
    const confirmationRef = this.modal.open(ConfirmationModalComponent);

    confirmationRef.componentInstance.content = 'Are you sure you want to delete this page \'' + this.page!.Name + '\'? All questions related to this page will be deleted from the test.';

    confirmationRef.result
      .then((ok: number) => {
        if (ok === 1) {
          //const index = this.pages.indexOf(page);
          //this.loading[index] = true;
          this.httpClient.delete('api/page/delete/' + this.page!.Id, { params: { auth: true, testId: this.page!.TestId } }).subscribe({
            next: data => {
              this.router.navigate(['/test-edit', this.page!.TestId]);
            },
            error: (error: HttpErrorResponse) => {
              //this.loading[index] = false;
            }
          });
        }
      })
      .catch(() => { });
  }

  OnDeleteQuestion(pageQuestion: PageQuestion): void {
    const confirmationRef = this.modal.open(ConfirmationModalComponent);

    confirmationRef.componentInstance.content = 'Are you sure you want to delete question #' + pageQuestion.Position + '?';

    confirmationRef.result
      .then((ok: number) => {
        if (ok === 1) {
          this.httpClient.delete('api/page-question/delete/' + pageQuestion.Id, { params: { auth: true } }).subscribe({
            next: data => {
              const index = this.page!.PageQuestions.findIndex(pq => pq.Id === pageQuestion.Id);
              this.page!.PageQuestions.splice(index, 1);
              //for (let i = index; i < this.testQuestions.length; i++) {
              //  this.testQuestions[i].Position--;
              //}
            },
            error: (error: HttpErrorResponse) => {
              
            }
          });
        }
      })
      .catch(() => { });
  }

}
