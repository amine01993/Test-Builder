import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FreeTextAddComponent } from './free-text-add.component';

describe('FreeTextAddComponent', () => {
  let component: FreeTextAddComponent;
  let fixture: ComponentFixture<FreeTextAddComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ FreeTextAddComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FreeTextAddComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
