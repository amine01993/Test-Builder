import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { Page } from '../test.model';
import { limitValidator } from './page-settings.validator';

@Component({
  selector: 'app-page-settings',
  templateUrl: './page-settings.component.html',
  styleUrls: ['./page-settings.component.css']
})
export class PageSettingsComponent implements OnInit {

  testId: number | undefined;
  page: Page | undefined;
  position: number | undefined;
  timeUnit: string = 'Seconds';
  serverErrors: { [key: string]: string } = {};
  submitting: boolean = false;

  pageForm: FormGroup = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.maxLength(30)]),
    shuffle: new FormControl(false),
    timeLimit: new FormControl(false),
    limit: new FormControl(0)
  }, [limitValidator()]);

  get name(): FormControl {
    return this.pageForm.get('name') as FormControl;
  }

  get timeLimit(): FormControl {
    return this.pageForm.get('timeLimit') as FormControl;
  }

  get limit(): FormControl {
    return this.pageForm.get('limit') as FormControl;
  }

  constructor(
    private httpClient: HttpClient,
    public activeModal: NgbActiveModal
  ) { }

  ngOnInit(): void {
    if (this.page) {
      this.pageForm.patchValue({
        name: this.page.Name,
        shuffle: this.page.Shuffle,
        limit: this.page.Limit ?? 0,
        timeLimit: this.page.Limit === null ? false : true
      });
    }
  }

  private getTimeLimit() {
    if (this.pageForm.value.timeLimit) {
      const limit = this.timeUnit === 'Seconds' ? (+this.pageForm.value.limit) : (+this.pageForm.value.limit) * 60;
      return limit;
    }
    return null;
  }

  ChangeTimeUnit() {
    switch (this.timeUnit) {
      case 'Seconds':
        if (this.pageForm.get('limit')?.valid) {
          const limit = Math.floor((+this.pageForm.get('limit')?.value) / 60);
          this.pageForm.patchValue({ limit });
        }
        this.timeUnit = 'Minutes';
        break;
      case 'Minutes':
        if (this.pageForm.get('limit')?.valid) {
          const limit = (+this.pageForm.get('limit')?.value) * 60;
          this.pageForm.patchValue({ limit });
        }
        this.timeUnit = 'Seconds';
        break;
      default:
        break;
    }
  }

  OnSubmit(): void {
    console.log(this.pageForm);
    if (this.pageForm.valid) {
      this.submitting = true;
      this.serverErrors = {};
      this.httpClient.post<Page>('api/page/add', {
        Id: this.page?.Id ?? 0,
        Name: this.pageForm.value.name,
        Shuffle: this.pageForm.value.shuffle,
        Limit: this.getTimeLimit(),
        TestId: this.testId,
        Position: this.page ? undefined : (this.position ?? 0)
      }, { params: { auth: true } }).subscribe({
        next: data => {
          this.page = data;
          this.activeModal.close(this.page);
          this.submitting = false;
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 422) {
            this.serverErrors = error.error;
          }
          this.submitting = false;
        }
      });
    }
  }

}
