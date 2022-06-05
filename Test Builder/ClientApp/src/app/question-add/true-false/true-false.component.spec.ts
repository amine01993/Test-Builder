import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TrueFalseAddComponent } from './true-false.component';

describe('TrueFalseComponent', () => {
  let component: TrueFalseAddComponent;
  let fixture: ComponentFixture<TrueFalseAddComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TrueFalseAddComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TrueFalseAddComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
