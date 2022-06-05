import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MultipleChoiceAddComponent } from './multiple-choice.component';

describe('MultipleChoiceComponent', () => {
  let component: MultipleChoiceAddComponent;
  let fixture: ComponentFixture<MultipleChoiceAddComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MultipleChoiceAddComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MultipleChoiceAddComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
