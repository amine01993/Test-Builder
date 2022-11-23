import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Subject } from "rxjs";
import { QuestionType } from "./question.model";

@Injectable({
  providedIn: 'root'
})
export class QuestionService {
  questionTypes: Subject<QuestionType[]> = new Subject;
  loadedTypes: boolean = false;

  constructor(
    private httpClient: HttpClient
  ) { }

  LoadQuestionTypes() {
    if (!this.loadedTypes) {
      this.httpClient.get<QuestionType[]>('api/question/types').subscribe({
        next: questionTypes => {
          this.loadedTypes = true;
          this.questionTypes.next(questionTypes);
        }
      });
    }
  }
}

