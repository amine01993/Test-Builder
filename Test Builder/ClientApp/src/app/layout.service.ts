import { Injectable } from "@angular/core";
import { Subject } from "rxjs";

@Injectable({
  providedIn: 'root'
})
export default class LayoutService {

  subject: Subject<string> = new Subject();

  constructor() { }

  setLayout(type: string) {
    this.subject.next(type);
  }
}
