import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormArray, FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AngularEditorConfig } from '@kolkov/angular-editor';
import { Subscription } from 'rxjs';
import { Category } from '../../category/category.model';
import { CategoryService } from '../../category/category.service';
import { editorConfig } from '../../editor.config';
import { Question } from '../question.model';
import { pointsValidator } from '../question.validator';

@Component({
  selector: 'app-essay-add',
  templateUrl: './essay-add.component.html',
  styleUrls: ['./essay-add.component.css']
})
export class EssayAddComponent implements OnInit, OnDestroy {

  _editorConfig: AngularEditorConfig = editorConfig;
  categories: Category[] = [];
  _categories: Category[] = [];
  _subCategories: Category[] = [];
  categoriesSub: Subscription | undefined;
  serverErrors: { [key: string]: string } = {};
  submitting: boolean = false;
  pageId: number = 0;
  questionId: number = 0;
  _question: Question | undefined;
  loading: boolean = false;

  essay: FormGroup = new FormGroup({
    question: new FormControl('', [Validators.required, Validators.minLength(5)]),
    category: new FormControl(1),
    subCategory: new FormControl(2),
    points: new FormControl(1, [pointsValidator()]),
  });

  get question(): FormControl {
    return this.essay.get('question') as FormControl;
  }
  get points(): FormControl {
    return this.essay.get('points') as FormControl;
  }

  constructor(
    private httpClient: HttpClient,
    private route: ActivatedRoute,
    private router: Router,
    private cateogoryService: CategoryService
  ) { }

  ngOnInit(): void {
    this.route.parent!.params.subscribe({
      next: params => {
        this.pageId = +params['page-id'];
      }
    });

    this.route.params.subscribe({
      next: params => {
        //console.log('params', params, +params['id']);
        if (+params['id']) {
          this.questionId = +params['id'];
          if (this.questionId > 0) {
            this.SetLoading(true);
            
            this.httpClient.get<Question>('api/question/' + this.questionId, { params: { auth: true } }).subscribe({
              next: question => {
                this._question = question;

                // set question in the form
                this.essay.patchValue({
                  question: question._Question,
                  points: question.Points,
                });

                this.SetSubCategory(question.CategoryId);
                this.SetLoading(false);
              },
              error: (error: HttpErrorResponse) => {
                console.log('post error', error);
                if (error.status === 404) {
                  console.log('post error - question doesn\'t exist');
                }
                this.loading = false;
                this.questionId = 0;
              }
            });
          }
        }
      }
    });

    this.categoriesSub = this.cateogoryService.categories.subscribe({
      next: (categories) => {
        this.categories = categories;
        this._categories = categories.filter(c => c.ParentId === null);
        if (this._question) {
          this.SetSubCategory(this._question.CategoryId);
        }
        else {
          this.OnCategoryChange();
        }
      }
    });
    this.cateogoryService.LoadCategories();
  }

  OnCategoryChange(subCategoryId?: number): void {
    const selectedCategory = +this.essay.value.category;
    this._subCategories = this.categories.filter(c => c.ParentId === selectedCategory);
    if (this._subCategories.length > 0) {
      if (subCategoryId) {
        this.essay.patchValue({ subCategory: subCategoryId });
      }
      else {
        this.essay.patchValue({ subCategory: this._subCategories[0].Id });
      }
    }
  }

  SetSubCategory(subCategoryId: number) {
    const subCategory = this.categories.find(c => c.Id === subCategoryId && c.ParentId !== null);
    if (subCategory) {
      this.essay.patchValue({ category: subCategory.ParentId });
      this.OnCategoryChange(subCategory.Id);
    }
  }

  SetLoading(loading: boolean) {
    this.loading = loading;
    this._editorConfig.editable = !loading;
  }

  OnSubmit(): void {
    console.log(this.essay);
    if (this.essay.valid) {
      this.submitting = true;

      let questionData = {
        Id: this.questionId,
        TypeId: 5, // Essay
        CategoryId: this.essay.value.subCategory,
        Points: this.essay.value.points,
        Penalty: null,
        Shuffle: null,
        Selection: null,
        _Question: this.essay.value.question,
        Answers: null,
      };

      if (this.questionId === 0) {
        this.httpClient.post<any>(
          'api/question/add' + (this.pageId ? '/' + this.pageId : ''), questionData, { params: { auth: true } })
          .subscribe({
            next: data => {
              // redirect to edit question
              this.submitting = false;
              if (this.pageId) {
                this.router.navigate(['/page-edit', this.pageId]);
              }
            },
            error: (error: HttpErrorResponse) => {
              console.log('post error', error);
              if (error.status === 422) {
                this.serverErrors = error.error;
                // display errors on top + scroll to the top
              }
              this.submitting = false;
            }
          });
      }
      else {
        this.httpClient.post<any>(
          'api/question/edit',
          questionData, { params: { auth: true } })
          .subscribe({
            next: data => {
              // redirect to question bank or previous page
              this.submitting = false;
            },
            error: (error: HttpErrorResponse) => {
              console.log('post error', error);
              if (error.status === 422) {
                this.serverErrors = error.error;
                // display errors on top + scroll to the top
              }
              this.submitting = false;
            }
          });
      }
    }
  }

  ngOnDestroy(): void {
    if (this.categoriesSub)
      this.categoriesSub.unsubscribe();
  }

}
