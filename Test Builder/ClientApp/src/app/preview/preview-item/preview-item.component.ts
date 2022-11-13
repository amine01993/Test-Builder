import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { PageQuestion } from '../../test/test.model';

@Component({
  selector: 'app-preview-item',
  templateUrl: './preview-item.component.html',
  styleUrls: ['./preview-item.component.scss']
})
export class PreviewItemComponent implements OnInit {

  @Input() pageQuestion!: PageQuestion;
  @Input() key!: number;

  private selectedAnswers: any[] = [];
  @Output() answersEvent: EventEmitter<{ questionId: number, answers: any[] }> = new EventEmitter;

  constructor() { }

  ngOnInit(): void {

  }

  onCheckChange(event: any) {
    if (this.pageQuestion.Question?.TypeId === 1) {
      if (this.pageQuestion.Question!.Selection) { // checkbox
        if (event.target.checked) {
          this.selectedAnswers.push(event.target.value);
        }
        else {
          const index = this.selectedAnswers.indexOf(event.target.value);
          if (index > -1) this.selectedAnswers.splice(index, 1);
        }
      }
      else { // radio
        this.selectedAnswers = [event.target.value];
      }
      this.answersEvent.emit({ questionId: this.pageQuestion.QuestionId!, answers: this.selectedAnswers });
    }
  }
}
