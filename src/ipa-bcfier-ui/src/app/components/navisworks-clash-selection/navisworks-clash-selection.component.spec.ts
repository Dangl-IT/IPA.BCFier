import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NavisworksClashSelectionComponent } from './navisworks-clash-selection.component';

describe('NavisworksClashSelectionComponent', () => {
  let component: NavisworksClashSelectionComponent;
  let fixture: ComponentFixture<NavisworksClashSelectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NavisworksClashSelectionComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(NavisworksClashSelectionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
