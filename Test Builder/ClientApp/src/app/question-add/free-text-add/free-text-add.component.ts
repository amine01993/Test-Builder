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
import { answerValidator, questionValidator } from '../question.validator';

@Component({
  selector: 'app-free-text-add',
  templateUrl: './free-text-add.component.html',
  styleUrls: ['./free-text-add.component.scss']
})
export class FreeTextAddComponent implements OnInit, OnDestroy {

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

  freeText: FormGroup = new FormGroup({
    question: new FormControl('', [Validators.required, Validators.minLength(5)]),
    category: new FormControl(1),
    subCategory: new FormControl(2),
    points: new FormControl(1),
    answers: new FormArray([
      this.getAnswerForm(),
      this.getAnswerForm()
    ])
  });

  private getAnswerForm(): FormGroup {
    return new FormGroup({
      answer: new FormControl('', [Validators.required]),
      points: new FormControl(1),
      penalty: new FormControl(0),
    }, [answerValidator()])
  }

  get question(): FormControl {
    return this.freeText.get('question') as FormControl;
  }

  get answers(): FormArray {
    return this.freeText.get('answers') as FormArray;
  }

  getAnswer(index: number): FormGroup {
    return this.answers.at(index) as FormGroup;
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
                if (question.Answers) {
                  const answers = this.answers;
                  
                  if (answers.length < question.Answers.length) {
                    let i = answers.length;
                    while (i < question.Answers.length) {
                      answers.push(this.getAnswerForm());
                      i++;
                    }
                  }
                  else if (answers.length > question.Answers.length) {
                    let i = answers.length - 1;
                    while (i >= question.Answers.length) {
                      answers.removeAt(i);
                      i--;
                    }
                  }
                }

                this.freeText.patchValue({
                  question: question._Question,
                  points: question.Points,
                  answers: question.Answers?.map(val => ({
                    points: val.Points,
                    penalty: val.Penalty,
                    answer: val._Answer,
                  }))
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
    const selectedCategory = +this.freeText.value.category;
    this._subCategories = this.categories.filter(c => c.ParentId === selectedCategory);
    if (this._subCategories.length > 0) {
      if (subCategoryId) {
        this.freeText.patchValue({ subCategory: subCategoryId });
      }
      else {
        this.freeText.patchValue({ subCategory: this._subCategories[0].Id });
      }
    }
  }

  SetSubCategory(subCategoryId: number) {
    const subCategory = this.categories.find(c => c.Id === subCategoryId && c.ParentId !== null);
    if (subCategory) {
      this.freeText.patchValue({ category: subCategory.ParentId });
      this.OnCategoryChange(subCategory.Id);
    }
  }

  SetLoading(loading: boolean) {
    this.loading = loading;
    this._editorConfig.editable = !loading;
  }

  AddMoreOptions(): void {
    this.answers.push(this.getAnswerForm());
    this.RecalculatePoints();
  }

  OnDeleteAnswer(index: number): void {
    this.answers.removeAt(index);
    this.RecalculatePoints();
  }

  RecalculatePoints() {
    let maxPointsValue = 0;
    for (let answer of this.freeText.value.answers) {
      maxPointsValue = Math.max(+answer.points, maxPointsValue);
    }
    this.freeText.patchValue({ points: maxPointsValue });
  }

  OnSubmit(): void {
    console.log(this.freeText);
    if (this.freeText.valid) {
      this.submitting = true;

      let _answers = [];
      for (const answer of this.freeText.value.answers) {
        _answers.push({
          _Answer: answer.answer,
          Match: null,
          Points: answer.points,
          Penalty: answer.penalty,
          Correct: true
        });
      }

      let questionData = {
        Id: this.questionId,
        TypeId: 4, // Free text
        CategoryId: this.freeText.value.subCategory,
        Points: this.freeText.value.points,
        Penalty: null,
        Shuffle: null,
        Selection: null,
        _Question: this.freeText.value.question,
        Answers: _answers,
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
