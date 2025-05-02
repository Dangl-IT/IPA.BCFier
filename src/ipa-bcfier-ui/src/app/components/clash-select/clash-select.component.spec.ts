import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ClashSelectComponent } from './clash-select.component';

describe('ClashSelectComponent', () => {
  let component: ClashSelectComponent;
  let fixture: ComponentFixture<ClashSelectComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ClashSelectComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ClashSelectComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
