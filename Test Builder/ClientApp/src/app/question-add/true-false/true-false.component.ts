import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormArray, FormControl, FormGroup, Validators } from '@angular/forms';
import { AngularEditorConfig } from '@kolkov/angular-editor';
import { Subscription } from 'rxjs';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';

import { editorConfig } from '../../editor.config';
import { Category } from '../../category/category.model';
import { answerValidator } from '../question.validator';
import { CategoryService } from '../../category/category.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Question } from '../question.model';

@Component({
  selector: 'app-true-false-add',
  templateUrl: './true-false.component.html',
  styleUrls: ['./true-false.component.scss']
})
export class TrueFalseAddComponent implements OnInit, OnDestroy {

  categories: Category[] = [];
  _categories: Category[] = [];
  _subCategories: Category[] = [];
  _question: Question | undefined;

  _editorConfig: AngularEditorConfig = editorConfig;
  categoriesSub: Subscription | undefined;
  serverErrors: { [key: string]: string } = {};
  submitting: boolean = false;
  pageId: number = 0;
  questionId: number = 0;
  loading: boolean = false;

  trueFalse: FormGroup = new FormGroup({
    question: new FormControl('', [Validators.required, Validators.minLength(5)]),
    points: new FormControl(1),
    penalty: new FormControl(0),
    correct: new FormControl(0),
    answers: new FormArray([
      new FormControl('True', [Validators.required]),
      new FormControl('False', [Validators.required])
    ]),
    shuffle: new FormControl(0),
    category: new FormControl(1),
    subCategory: new FormControl(2)
  }, [answerValidator()]);

  get question(): FormArray {
    return this.trueFalse.get('question') as FormArray;
  }

  get answers(): FormArray {
    return this.trueFalse.get('answers') as FormArray;
  }

  get correct(): FormControl {
    return this.trueFalse.get('correct') as FormControl;
  }

  get points(): FormControl {
    return this.trueFalse.get('points') as FormControl;
  }

  get penalty(): FormControl {
    return this.trueFalse.get('penalty') as FormControl;
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
                this.trueFalse.patchValue({
                  question: question._Question,
                  points: question.Points,
                  penalty: question.Penalty,
                  shuffle: question.Shuffle,
                  selection: null,
                  answers: question.Answers?.map(val => val._Answer),
                  correct: question.Answers?.findIndex(val => val.Correct),
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
      next: categories => {
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
    const selectedCategory = +this.trueFalse.value.category;
    this._subCategories = this.categories.filter(c => c.ParentId === selectedCategory);
    if (this._subCategories.length > 0) {
      if (subCategoryId) {
        this.trueFalse.patchValue({ subCategory: subCategoryId });
      }
      else {
        this.trueFalse.patchValue({ subCategory: this._subCategories[0].Id });
      }
    }
  }

  SetSubCategory(subCategoryId: number) {
    const subCategory = this.categories.find(c => c.Id === subCategoryId && c.ParentId !== null);
    if (subCategory) {
      this.trueFalse.patchValue({ category: subCategory.ParentId });
      this.OnCategoryChange(subCategory.Id);
    }
  }

  SetLoading(loading: boolean) {
    this.loading = loading;
    this._editorConfig.editable = !loading;
  }

  RecalculatePoints() {
    this.trueFalse.patchValue({ points: this.trueFalse.value.points });
  }

  OnSubmit(): void {
    console.log(this.trueFalse);
    if (this.trueFalse.valid) {
      this.submitting = true;

      const _answers = this.trueFalse.value.answers;

      let questionData = {
        Id: this.questionId,
        TypeId: 2, // True False
        CategoryId: this.trueFalse.value.subCategory,
        Points: this.trueFalse.value.points,
        Penalty: this.trueFalse.value.penalty,
        Shuffle: this.trueFalse.value.shuffle,
        Selection: null,
        _Question: this.trueFalse.value.question,
        Answers: [
          {
            _Answer: _answers[0],
            Match: null,
            Points: null,
            Penalty: null,
            Correct: this.trueFalse.value.correct === 0 ? true : false
          },
          {
            _Answer: _answers[1],
            Match: null,
            Points: null,
            Penalty: null,
            Correct: this.trueFalse.value.correct === 1 ? true : false
          }
        ],
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
