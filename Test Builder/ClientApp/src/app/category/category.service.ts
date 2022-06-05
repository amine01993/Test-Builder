import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Subject } from "rxjs";
import { Category } from "./category.model";

@Injectable({
  providedIn: 'root'
})
export class CategoryService {

  categories: Subject<Category[]> = new Subject;
  _categories: Category[] = [];

  constructor(
    private httpClient: HttpClient
  ) { }

  LoadCategories() {
    this.httpClient.get<Category[]>('api/category', { params: { auth: true } }).subscribe({
      next: categories => {
        this._categories = categories;
        this.categories.next(categories);
      }
    });
  }

}
