import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ElementSelectComponent } from './element-select.component';

describe('ElementSelectComponent', () => {
  let component: ElementSelectComponent;
  let fixture: ComponentFixture<ElementSelectComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ElementSelectComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ElementSelectComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
