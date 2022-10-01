import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ConfirmationModalComponent } from '../../confirmation-modal/confirmation-modal.component';
import { PageSettingsComponent } from '../page-settings/page-settings.component';
import { Page, Test } from '../test.model';

@Component({
  selector: 'app-test-edit',
  templateUrl: './test-edit.component.html',
  styleUrls: ['./test-edit.component.scss']
})
export class TestEditComponent implements OnInit {

  test: Test | undefined;
  //pages: Page[] = [];

  serverErrors: { [key: string]: string } = {};
  loading: boolean[] = [];

  constructor(
    private route: ActivatedRoute,
    private httpClient: HttpClient,
    private modal: NgbModal
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe({
      next: params => {
        this.httpClient.get<Test>('api/test/' + params['id'], { params: { auth: true } }).subscribe({
          next: data => {
            this.test = data;
            this.loading = Array(this.test.Pages.length).fill(false);
          }
        });
      }
    });
  }

  OnAddNewPage(): void {
    const pageRef = this.modal.open(PageSettingsComponent, { size: 'lg' });

    pageRef.componentInstance.testId = this.test!.Id;
    pageRef.componentInstance.position = this.test!.Pages[this.test!.Pages.length - 1].Position + 1;

    pageRef.result
      .then(page => {
        console.log('page', page);
        this.test!.Pages.push(page);
        this.loading.push(false);
      })
      .catch(c => {
        console.log('catch', c);
      });
  }

  OnEditPageSettings(page: Page): void {
    const pageRef = this.modal.open(PageSettingsComponent, { size: 'lg' });

    pageRef.componentInstance.testId = this.test?.Id;
    pageRef.componentInstance.page = page;

    pageRef.result
      .then((_page: Page) => {
        console.log('page', _page);
        page.Name = _page.Name;
        page.Shuffle = _page.Shuffle;
        page.Limit = _page.Limit;
      })
      .catch(c => {
        console.log('catch', c);
      });
  }

  OnDeletePage(page: Page): void {
    const confirmationRef = this.modal.open(ConfirmationModalComponent);

    confirmationRef.componentInstance.content = 'Are you sure you want to delete this page \'' + page.Name + '\'? All questions related to this page will be deleted from the test.';

    confirmationRef.result
      .then((ok: number) => {
        if (ok === 1) {
          const index = this.test!.Pages.indexOf(page);
          this.loading[index] = true;
          this.httpClient.delete('api/page/delete/' + page.Id, { params: { auth: true, testId: page.TestId } }).subscribe({
            next: data => {
              //this.loading[index] = false;
              this.test!.Pages.splice(index, 1);
              this.loading.splice(index, 1);
            },
            error: (error: HttpErrorResponse) => {
              this.loading[index] = false;
            }
          });
        }
      })
      .catch(() => {});
  }

  drop(event: CdkDragDrop<any>): void {
    console.log(event);
    if (event.previousIndex === event.currentIndex || !this.test)
      return;

    moveItemInArray(this.test!.Pages, event.previousIndex, event.currentIndex);

    const pagePositions = [];
    for (let i = 0; i < this.test!.Pages.length; i++) {
      this.test!.Pages[i].Position = i;
      pagePositions.push({ Id: this.test!.Pages[i].Id, _Position: this.test!.Pages[i].Position });
    }

    this.httpClient.post<any>('api/page/positions', pagePositions, {
      params: { auth: true, testId: this.test!.Id }
    }).subscribe({
      next: data => {}
    });
  }

}
