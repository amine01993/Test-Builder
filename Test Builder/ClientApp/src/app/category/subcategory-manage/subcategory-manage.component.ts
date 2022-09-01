import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { Subscription } from 'rxjs';
import { Category } from '../category.model';
import { CategoryService } from '../category.service';

@Component({
  selector: 'app-subcategory-manage',
  templateUrl: './subcategory-manage.component.html',
  styleUrls: ['./subcategory-manage.component.scss']
})
export class SubcategoryManageComponent implements OnInit, OnDestroy {

  @Input() category: Category | undefined;
  @Input() categoryList: Category[] | undefined;

  subCategoryForm: FormGroup = new FormGroup({
    category: new FormControl(1),
    name: new FormControl('', [Validators.required, Validators.maxLength(30)])
  });
  submitting: boolean = false;
  categorySub: Subscription | undefined;
  serverErrors: { [key: string]: string[] } = {};

  get name(): FormControl {
    return this.subCategoryForm.get('name') as FormControl;
  }

  constructor(
    private httpClient: HttpClient,
    public activeModal: NgbActiveModal,
    private categoryService: CategoryService
  ) { }

  ngOnInit(): void {
    if (!this.categoryList) {
      this.categorySub = this.categoryService.categories.subscribe({
        next: categoryList => {
          this.categoryList = categoryList;
        }
      });
      this.categoryService.LoadCategories();
    }

    if (this.category) {
      this.subCategoryForm.patchValue({
        category: this.category?.ParentId,
        name: this.category?.Name
      });
    }
  }

  OnSubmit(): void {
    if (this.subCategoryForm.valid) {
      this.submitting = true;
      this.serverErrors = {}
      this.httpClient.post<Category[]>('api/category/add', {
        Id: (this.category ? this.category.Id : 0),
        Name: this.subCategoryForm.value.name,
        ParentId: this.subCategoryForm.value.category,
      }, { params: { auth: true } }).subscribe({
        next: (categories) => {
          this.activeModal.close(categories);
        },
        error: (error: HttpErrorResponse) => {
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
