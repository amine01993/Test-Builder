import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EssayAddComponent } from './essay-add.component';

describe('EssayAddComponent', () => {
  let component: EssayAddComponent;
  let fixture: ComponentFixture<EssayAddComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EssayAddComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(EssayAddComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
