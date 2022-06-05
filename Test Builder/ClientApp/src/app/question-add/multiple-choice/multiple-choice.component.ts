import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormArray, FormControl, FormGroup, Validators } from '@angular/forms';
import { AngularEditorConfig } from '@kolkov/angular-editor';
import { Subscription } from 'rxjs';

import { editorConfig } from '../../editor.config'
import { Category } from '../../category/category.model';
import { answerValidator, questionValidator } from '../question.validator';
import { CategoryService } from '../../category/category.service';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { Question } from '../question.model';

@Component({
  selector: 'app-multiple-choice-add',
  templateUrl: './multiple-choice.component.html',
  styleUrls: ['./multiple-choice.component.css']
})
export class MultipleChoiceAddComponent implements OnInit, OnDestroy {

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

  multipleChoice: FormGroup = new FormGroup({
    question: new FormControl('', [Validators.required, Validators.minLength(5)]),
    answers: new FormArray([
      this.getAnswerForm(),
      this.getAnswerForm()
    ]),
    category: new FormControl(1),
    subCategory: new FormControl(2),
    points: new FormControl(0),
    shuffle: new FormControl(0),
    selection: new FormControl(0)
  }, [questionValidator()]);

  private getAnswerForm(): FormGroup {
    return new FormGroup({
      points: new FormControl(1),
      penalty: new FormControl(0),
      correct: new FormControl(false),
      answer: new FormControl('', [Validators.required])
    }, [answerValidator()]);
  }

  get question(): FormControl {
    return this.multipleChoice.get('question') as FormControl;
  }
  get answers(): FormArray {
    return this.multipleChoice.get('answers') as FormArray;
  }

  getAnswer(index: number): FormGroup {
    return this.answers.at(index) as FormGroup;
  }

  constructor(
    private httpClient: HttpClient,
    private route: ActivatedRoute,
    private router: Router,
    private categoryService: CategoryService
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

                this.multipleChoice.patchValue({
                  question: question._Question,
                  points: question.Points,
                  shuffle: question.Shuffle,
                  selection: question.Selection,
                  answers: question.Answers?.map(val => ({
                    points: val.Points,
                    penalty: val.Penalty,
                    correct: val.Correct,
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

    this.categoriesSub = this.categoryService.categories.subscribe({
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
    this.categoryService.LoadCategories();
  }

  OnCategoryChange(subCategoryId?: number): void {
    const selectedCategory = +this.multipleChoice.value.category;
    this._subCategories = this.categories.filter(c => c.ParentId === selectedCategory);
    if (this._subCategories.length > 0) {
      if (subCategoryId) {
        this.multipleChoice.patchValue({ subCategory: subCategoryId });
      }
      else {
        this.multipleChoice.patchValue({ subCategory: this._subCategories[0].Id });
      }
    }
  }

  SetSubCategory(subCategoryId: number) {
    const subCategory = this.categories.find(c => c.Id === subCategoryId && c.ParentId !== null);
    if (subCategory) {
      this.multipleChoice.patchValue({ category: subCategory.ParentId });
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

  OnDeleteAnswer(i: number): void {
    this.answers.removeAt(i);
    this.RecalculatePoints();
  }

  RecalculatePoints() {
    let pointsValue = 0;
    for (let answer of this.multipleChoice.value.answers) {
      if (answer.correct) {
        pointsValue += +answer.points;
      }
    }
    this.multipleChoice.patchValue({ points: pointsValue });
  }

  OnSubmit(): void {
    console.log(this.multipleChoice);
    if (this.multipleChoice.valid) {
      this.submitting = true;

      let _answers = [];
      for (const answer of this.multipleChoice.value.answers) {
        _answers.push({
          _Answer: answer.answer,
          Match: null,
          Points: answer.points,
          Penalty: answer.penalty,
          Correct: answer.correct
        });
      }

      let questionData = {
        Id: this.questionId,
        TypeId: 1, // Multiple choice
        CategoryId: this.multipleChoice.value.subCategory,
        Points: this.multipleChoice.value.points,
        Penalty: null,
        Shuffle: this.multipleChoice.value.shuffle,
        Selection: this.multipleChoice.value.selection,
        _Question: this.multipleChoice.value.question,
        Answers: _answers,
      };

      if (this.questionId === 0) {
        this.httpClient.post<any>(
          'api/question/add' + (this.pageId ? '/' + this.pageId : ''),
          questionData, { params: { auth: true } })
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
