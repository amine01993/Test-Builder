import { HttpClient, HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { OnDestroy } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Subscription } from 'rxjs';
import { Category } from '../category/category.model';
import { CategoryService } from '../category/category.service';
import { ConfirmationModalComponent } from '../confirmation-modal/confirmation-modal.component';
import { DataResult, QuestionType } from '../question-add/question.model';
import { QuestionService } from '../question-add/question.service';
import { Page, TestQuestion } from '../test/test.model';

@Component({
  selector: 'app-question-bank',
  templateUrl: './question-bank.component.html',
  styleUrls: ['./question-bank.component.css']
})
export class QuestionBankComponent implements OnInit, OnDestroy {

  //test: Test | undefined;
  currentpage: Page | undefined;

  questionTypes: QuestionType[] = [];
  categories: Category[] = [];
  _categories: Category[] = [];
  _subCategories: Category[] = [];

  searching: boolean = false;
  categorySub: Subscription | undefined;
  questionSub: Subscription | undefined;

  page: number = 1;
  pageSize: number = 10;
  filter: { [key: string]: string } = {};
  questions: TestQuestion[] = [];

  pageControl: FormControl = new FormControl(1);
  pageSizeControl: FormControl = new FormControl(10);
  pages: number[] = [];
  pageSizes: number[] = [5, 10, 20, 50];


  filterForm: FormGroup = new FormGroup({
    status: new FormControl(0),
    questionType: new FormControl(0),
    category: new FormControl(0),
    subCategory: new FormControl(0),
    searchTerm: new FormControl(''),
  });

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private httpClient: HttpClient,
    private questionService: QuestionService,
    private categoryService: CategoryService
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe({
      next: params => {
        const pageId = +params['page-id'];
        if (pageId) {
          //this.httpClient.get<{ page: Page }>('api/test/' + params['id'], { params: { auth: true } }).subscribe({
          //  next: data => {
          //    this.page = data.page;
          //  }
          //});
        }
      }
    });

    this.questionSub = this.questionService.questionTypes.subscribe({
      next: types => {
        this.questionTypes = types;
      }
    });
    this.questionService.LoadQuestionTypes();

    this.categorySub = this.categoryService.categories.subscribe({
      next: categories => {
        this.categories = categories;
        this._categories = categories.filter(c => c.ParentId === null);
        this.OnCategoryChange();
      }
    });
    this.categoryService.LoadCategories();

    this.route.queryParams.subscribe({
      next: queryParams => {
        this.page = queryParams.hasOwnProperty('page') ? +queryParams['page'] : 1;
        this.pageSize = queryParams.hasOwnProperty('pageSize') ? +queryParams['pageSize'] : 10;

        this.pageControl.setValue(this.page);
        this.pageSizeControl.setValue(this.pageSize);

        if (queryParams.hasOwnProperty('_filter')) {
          this.filter = this.decodeParam(queryParams['_filter']);
        }

        if (!this.filter.hasOwnProperty('status')) {
          this.filter['status'] = '0';
        }

        if (!this.filter.hasOwnProperty('type')) {
          this.filter['type'] = '0';
        }

        if (!this.filter.hasOwnProperty('category')) {
          this.filter['category'] = '0';
        }

        if (!this.filter.hasOwnProperty('subCategory')) {
          this.filter['subCategory'] = '0';
        }

        if (!this.filter.hasOwnProperty('term')) {
          this.filter['term'] = '';
        }

        console.log('this.filter', this.filter);        
        this.LoadData();
      }
    });

  }

  private encodeParam(param: { [key: string]: string }): string {
    let arr = [], str = '';
    for (const key in param) {
      arr.push(key + '=' + param[key]);
    }
    str = arr.join(';');
    return str;
  }

  private decodeParam(str: string): { [key: string]: string } {
    const dict: { [key: string]: string } = {};
    const tokens = str === '' ? [] : str.split(';');
    for (const token of tokens) {
      const [key, value] = token.split('=');
      dict[key] = value;
    }
    return dict;
  }

  OnCategoryChange(): void {
    const selectedCategory = +this.filterForm.value.category;
    if (selectedCategory === 0) {
      this._subCategories = this.categories.filter(c => c.ParentId !== null);
    }
    else {
      this._subCategories = this.categories.filter(c => c.ParentId === selectedCategory);
    }
    this.filterForm.patchValue({ subCategory: 0 });
    //if (this._subCategories.length > 0) {
    //  this.filterForm.patchValue({ subCategory: this._subCategories[0].Id });
    //}
  }

  LoadData(): void {
    this.httpClient.get<DataResult<TestQuestion>>('api/question/search', {
      params: {
        auth: true,
        page: this.page, pageSize: this.pageSize,
        _filter: this.encodeParam(this.filter)
      }
    })
    .subscribe({
      next: data => {
        this.questions = data.Data;
        this.pages = Array(Math.ceil(data.Total / this.pageSize)).fill(0).map((v, i) => i + 1);
      }
    });
  }

  OnPageChange(): void {
    this.page = +this.pageControl.value;

    this.router.navigate(['/question-bank'], {
      queryParams: {
        _filter: this.encodeParam(this.filter),
        page: this.page,
        pageSize: this.pageSize
      }
    });
  }

  OnPreviousPage(): void {
    this.router.navigate(['/question-bank'], {
      queryParams: {
        _filter: this.encodeParam(this.filter),
        page: this.page - 1,
        pageSize: this.pageSize
      }
    });
  }

  OnNextPage(): void {
    this.router.navigate(['/question-bank'], {
      queryParams: {
        _filter: this.encodeParam(this.filter),
        page: this.page + 1,
        pageSize: this.pageSize
      }
    });
  }

  OnPageSizeChange(): void {
    this.pageSize = +this.pageSizeControl.value;
    this.router.navigate(['/question-bank'], {
      queryParams: {
        _filter: this.encodeParam(this.filter),
        page: 1,
        pageSize: this.pageSize
      }
    });
  }

  ReloadData(): void {
    this.LoadData();
  }

  OnFilterSubmit(): void {
    console.log(this.filterForm);
    if (this.filterForm.valid) {
      this.filter = {
        status: this.filterForm.value.status,
        type: this.filterForm.value.questionType,
        category: this.filterForm.value.category,
        subCategory: this.filterForm.value.subCategory,
        term: this.filterForm.value.searchTerm,
      };

      this.router.navigate(['/question-bank'], {
        queryParams: {
          _filter: this.encodeParam(this.filter)
        }
      });
    }
  }

  ngOnDestroy(): void {
    if (this.categorySub)
      this.categorySub.unsubscribe();
    if (this.questionSub)
      this.questionSub.unsubscribe();
  }

}
