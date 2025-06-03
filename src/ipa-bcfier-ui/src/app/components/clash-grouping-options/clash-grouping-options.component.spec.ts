import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ClashGroupingOptionsComponent } from './clash-grouping-options.component';

describe('ClashGroupingOptionsComponent', () => {
  let component: ClashGroupingOptionsComponent;
  let fixture: ComponentFixture<ClashGroupingOptionsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ClashGroupingOptionsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ClashGroupingOptionsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
