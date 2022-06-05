import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AngularEditorConfig } from '@kolkov/angular-editor';
import { Subscription } from 'rxjs';
import { Category } from '../../category/category.model';
import { CategoryService } from '../../category/category.service';
import { editorConfig } from '../../editor.config';

@Component({
  selector: 'app-test-add',
  templateUrl: './test-add.component.html',
  styleUrls: ['./test-add.component.css']
})
export class TestAddComponent implements OnInit, OnDestroy {

  _editorConfig: AngularEditorConfig = editorConfig;
  categories: Category[] = [];
  _categories: Category[] = [];
  _subCategories: Category[] = [];
  categoriesSub: Subscription | undefined;
  serverErrors: { [key: string]: string } = {};
  submitting: boolean = false;

  test: FormGroup = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.minLength(5)]),
    category: new FormControl(1),
    subCategory: new FormControl(2),
    introduction: new FormControl('')
  });

  get name(): FormControl {
    return this.test.get('name') as FormControl;
  }

  constructor(
    private httpClient: HttpClient,
    private router: Router,
    private categoryService: CategoryService
  ) { }

  ngOnInit(): void {
    this._editorConfig.minHeight = '100px';
    this.categoriesSub = this.categoryService.categories.subscribe({
      next: (categories) => {
        this.categories = categories;
        this._categories = categories.filter(c => c.ParentId === null);
        this.OnCategoryChange();
      }
    });
    this.categoryService.LoadCategories();
  }

  OnCategoryChange(): void {
    const selectedCategory = +this.test.value.category;
    this._subCategories = this.categories.filter(c => c.ParentId === selectedCategory);
    if (this._subCategories.length > 0) {
      this.test.patchValue({ subCategory: this._subCategories[0].Id });
    }
  }

  OnSubmit(): void {
    console.log(this.test);
    if (this.test.valid) {
      this.submitting = true;
      this.serverErrors = {};

      this.httpClient.post<any>('api/test/add', {
        Name: this.test.value.name,
        CategoryId: this.test.value.subCategory,
        Introduction: this.test.value.introduction,
        }, { params: { auth: true } }).subscribe({
        next: data => {
          if (data.result) {
            // save id move to next step
            this.router.navigate(['/test-edit', data.id]);
          }
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

  ngOnDestroy(): void {
    if (this.categoriesSub)
      this.categoriesSub.unsubscribe();
  }
}
