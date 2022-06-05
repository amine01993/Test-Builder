import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MatchingAddComponent } from './matching-add.component';

describe('MatchingAddComponent', () => {
  let component: MatchingAddComponent;
  let fixture: ComponentFixture<MatchingAddComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MatchingAddComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MatchingAddComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
