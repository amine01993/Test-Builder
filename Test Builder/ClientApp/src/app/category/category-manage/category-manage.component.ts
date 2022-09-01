import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { Subscription } from 'rxjs';
import { Category } from '../category.model';

@Component({
  selector: 'app-category-manage',
  templateUrl: './category-manage.component.html',
  styleUrls: ['./category-manage.component.scss']
})
export class CategoryManageComponent implements OnInit, OnDestroy {

  @Input() category: Category | undefined;

  categoryForm: FormGroup = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.maxLength(30)])
  });
  submitting: boolean = false;
  categorySub: Subscription | undefined;
  serverErrors: { [key: string]: string[] } = {};

  get name(): FormControl {
    return this.categoryForm.get('name') as FormControl;
  }

  constructor(
    private httpClient: HttpClient,
    public activeModal: NgbActiveModal,
  ) { }

  ngOnInit(): void {
    if (this.category) {
      this.categoryForm.patchValue({
        name: this.category?.Name
      });
    }
  }

  OnSubmit(): void {
    if (this.categoryForm.valid) {
      this.submitting = true;
      this.serverErrors = {};
      this.httpClient.post<Category[]>('api/category/add', {
        Id: (this.category ? this.category.Id : 0),
        Name: this.categoryForm.value.name,
        ParentId: null,
      }, { params: { auth: true } }).subscribe({
        next: (categories) => {
          this.activeModal.close(categories);
        },
        error: (error: HttpErrorResponse) => {
          console.log(error);
          if (error.status === 422) {
            this.serverErrors = error.error;
          }
          this.submitting = false;
        },
        complete: () => {
          this.submitting = false;
        }
      });
    }
  }

  ngOnDestroy(): void {
    if (this.categorySub)
      this.categorySub.unsubscribe();
  }
}
