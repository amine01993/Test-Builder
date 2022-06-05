import { HttpClient } from '@angular/common/http';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Subscription } from 'rxjs';
import { ConfirmationModalComponent } from '../confirmation-modal/confirmation-modal.component';
import { CategoryManageComponent } from './category-manage/category-manage.component';
import { Category } from './category.model';
import { CategoryService } from './category.service';
import { SubcategoryManageComponent } from './subcategory-manage/subcategory-manage.component';

@Component({
  selector: 'app-category',
  templateUrl: './category.component.html',
  styleUrls: ['./category.component.css']
})
export class CategoryComponent implements OnInit, OnDestroy {

  categories: Category[] = [];
  categorySub: Subscription | undefined;

  constructor(
    private httpClient: HttpClient,
    private modal: NgbModal,
    private categoryService: CategoryService
  ) { }

  ngOnInit(): void {
    this.categorySub = this.categoryService.categories.subscribe({
      next: categories => {
        this.sortCategories(categories);
      }
    });
    this.categoryService.LoadCategories();
  }

  sortCategories(_categories: Category[]): void {
    this.categories = _categories.slice().sort((a, b) => {
      if (a.ParentId === null && b.ParentId === null) {
        return a.Id < b.Id ? -1 : 1;
      }
      if (a.ParentId === null && b.ParentId !== null) {
        return a.Id < b.ParentId ? -1 : 1;
      }
      if (a.ParentId !== null && b.ParentId === null) {
        return a.ParentId < b.Id ? -1 : 1;
      }
      if (a.ParentId !== null && b.ParentId !== null) {
        return a.ParentId < b.ParentId ? -1 : 1;
      }
      return 0;
    });
    console.log(this.categories);
  }

  AddSubCategory(category: Category | null): void {

    const modalRef = this.modal.open(SubcategoryManageComponent);
    modalRef.componentInstance.categoryList = this.categories.filter(c => c.ParentId === null);

    if (category) {
      modalRef.componentInstance.category = category;
    }

    modalRef.closed.subscribe({
      next: (result: Category[] | number) => {
        if (Array.isArray(result)) {
          this.sortCategories(result);
        }
      }
    });
  }

  AddCategory(category: Category | null): void {

    const modalRef = this.modal.open(CategoryManageComponent);

    if (category) {
      modalRef.componentInstance.category = category;
    }

    modalRef.closed.subscribe({
      next: (result: Category[] | number) => {
        if (Array.isArray(result)) {
          this.sortCategories(result);
        }
      }
    });
  }

  DeleteCategory(category: Category, btn: HTMLButtonElement) {
    console.log(category, btn);
    btn.disabled = true;

    const modalRef = this.modal.open(ConfirmationModalComponent);
    if (category.ParentId) {
      modalRef.componentInstance.title = `Deleting '${category.Name}' Subcategory`;
      modalRef.componentInstance.content = 'Would you like to delete this Subcategory ?';
    }
    else {
      modalRef.componentInstance.title = `Deleting '${category.Name}' Category`;
      modalRef.componentInstance.content = `Deleting this Category would result in deleting all its subcategories,
        do you still want to remove this Category ?`;
    }

    modalRef.result.then( result => {
      console.log('result', result);
      if (result === 1) {
        let params: any = { auth: true, id: category.Id };
        if (category.ParentId === null) {
          params.hasSubCategories = true;
        }
        this.httpClient.delete<Category[]>('api/category/delete', { params }).subscribe({
          next: categories => {
            this.sortCategories(categories);
          }
        });
      }
      btn.disabled = false;
    }).catch(err => {
      console.log('result - err', err);
      btn.disabled = false;
    });
    
  }

  ngOnDestroy(): void {
    if (this.categorySub)
      this.categorySub.unsubscribe();
  }
}
