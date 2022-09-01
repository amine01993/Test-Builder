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
  selector: 'app-matching-add',
  templateUrl: './matching-add.component.html',
  styleUrls: ['./matching-add.component.scss']
})
export class MatchingAddComponent implements OnInit, OnDestroy {

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

  matching: FormGroup = new FormGroup({
    question: new FormControl('Match the options below:', [Validators.required, Validators.minLength(5)]),
    category: new FormControl(1),
    subCategory: new FormControl(2),
    points: new FormControl(2),
    shuffle: new FormControl(0),
    answers: new FormArray([this.getAnswerForm(), this.getAnswerForm()])
  }, [questionValidator()]);

  private getAnswerForm(): FormGroup {
    return new FormGroup({
      points: new FormControl(1),
      penalty: new FormControl(0),
      clue: new FormControl('', [Validators.required]),
      match: new FormControl('', [Validators.required])
    }, [answerValidator()]);
  }

  get question(): FormControl {
    return this.matching.get('question') as FormControl;
  }

  get answers(): FormArray {
    return this.matching.get('answers') as FormArray;
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

                this.matching.patchValue({
                  question: question._Question,
                  points: question.Points,
                  shuffle: question.Shuffle,
                  answers: question.Answers?.map(val => ({
                    points: val.Points,
                    penalty: val.Penalty,
                    clue: val._Answer,
                    match: val.Match,
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
    const selectedCategory = +this.matching.value.category;
    this._subCategories = this.categories.filter(c => c.ParentId === selectedCategory);
    if (this._subCategories.length > 0) {
      if (subCategoryId) {
        this.matching.patchValue({ subCategory: subCategoryId });
      }
      else {
        this.matching.patchValue({ subCategory: this._subCategories[0].Id });
      }
    }
  }

  SetSubCategory(subCategoryId: number) {
    const subCategory = this.categories.find(c => c.Id === subCategoryId && c.ParentId !== null);
    if (subCategory) {
      this.matching.patchValue({ category: subCategory.ParentId });
      this.OnCategoryChange(subCategory.Id);
    }
  }

  SetLoading(loading: boolean) {
    this.loading = loading;
    this._editorConfig.editable = !loading;
  }

  AddMoreMatchingPairs(): void {
    this.answers.push(this.getAnswerForm());
    this.RecalculatePoints();
  }

  OnDeleteAnswer(index: number): void {
    this.answers.removeAt(index);
    this.RecalculatePoints();
  }

  RecalculatePoints() {
    let pointsValue = 0;
    for (let answer of this.matching.value.answers) {
      pointsValue += +answer.points;
    }
    this.matching.patchValue({ points: pointsValue });
  }

  OnSubmit(): void {
    console.log(this.matching);
    this.serverErrors = {};
    if (this.matching.valid) {
      this.submitting = true;

      let _answers = [];
      for (const answer of this.matching.value.answers) {
        _answers.push({
          _Answer: answer.clue,
          Match: answer.match,
          Points: answer.points,
          Penalty: answer.penalty,
          Correct: true
        });
      }

      let questionData = {
        Id: this.questionId,
        TypeId: 3, // Matching
        CategoryId: this.matching.value.subCategory,
        Points: this.matching.value.points,
        Penalty: null,
        Shuffle: this.matching.value.shuffle,
        Selection: null,
        _Question: this.matching.value.question,
        Answers: _answers,
      };

      if (this.questionId === 0) {
        this.httpClient.post<any>(
          'api/question/add' + (this.pageId ? '/' + this.pageId : ''), questionData, { params: { auth: true } })
          .subscribe({
            next: data => {
              this.submitting = false;
              if (this.pageId) {
                this.router.navigate(['/page-edit', this.pageId]);
              }
            },
            error: (error: HttpErrorResponse) => {
              console.log('post error', error);
              if (error.status === 422) {
                this.serverErrors = error.error;
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
