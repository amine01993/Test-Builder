import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Subject } from "rxjs";
import { Category } from "./category.model";

@Injectable({
  providedIn: 'root'
})
export class CategoryService {

  categories: Subject<Category[]> = new Subject;
  loaded: boolean = false;

  constructor(
    private httpClient: HttpClient
  ) { }

  LoadCategories() {
    if (!this.loaded) {
      this.httpClient.get<Category[]>('api/category', { params: { auth: true } }).subscribe({
        next: categories => {
          this.loaded = true;
          this.categories.next(categories);
        }
      });
    }
  }

}
