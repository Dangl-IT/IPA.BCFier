import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ElementsViewpointComponent } from './elements-viewpoint.component';

describe('ElementsViewpointComponent', () => {
  let component: ElementsViewpointComponent;
  let fixture: ComponentFixture<ElementsViewpointComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ElementsViewpointComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ElementsViewpointComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
