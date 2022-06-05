import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Subject } from "rxjs";
import { QuestionType } from "./question.model";

@Injectable({
  providedIn: 'root'
})
export class QuestionService {
  questionTypes: Subject<QuestionType[]> = new Subject;

  constructor(
    private httpClient: HttpClient
  ) { }

  LoadQuestionTypes() {
    //this.questionTypes.next([
    //  { name: 'Multiple Choice', icon: 'card-checklist', link: 'multiple-choice' },
    //  { name: 'True False', icon: 'check|x', link: 'true-false' },
    //  { name: 'Matching', icon: 'arrow-left-right', link: 'matching' },
    //  { name: 'Free Text', icon: 'input-cursor-text', link: 'free-text' },
    //  { name: 'Essay', icon: 'justify-left', link: 'essay' }
    //]);

    this.httpClient.get<QuestionType[]>('api/question/types').subscribe({
      next: questionTypes => {
        this.questionTypes.next(questionTypes);
      }
    });
  }
}

