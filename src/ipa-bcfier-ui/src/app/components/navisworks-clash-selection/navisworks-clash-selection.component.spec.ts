import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NavisworksClashSelectionComponent } from './navisworks-clash-selection.component';
import { AppTestingModule } from '../../app.testing.module';

describe('NavisworksClashSelectionComponent', () => {
  let component: NavisworksClashSelectionComponent;
  let fixture: ComponentFixture<NavisworksClashSelectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NavisworksClashSelectionComponent, AppTestingModule]
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
