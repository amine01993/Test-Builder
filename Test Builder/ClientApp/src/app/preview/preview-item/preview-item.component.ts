import { Component, Input, OnInit } from '@angular/core';
import { TestQuestion } from '../../test/test.model';

@Component({
  selector: 'app-preview-item',
  templateUrl: './preview-item.component.html',
  styleUrls: ['./preview-item.component.scss']
})
export class PreviewItemComponent implements OnInit {

  @Input() question!: TestQuestion;
  @Input() key!: number;

  constructor() { }

  ngOnInit(): void {
  }

}
