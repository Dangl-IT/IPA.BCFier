import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NavisworksClashesLoadingScreenComponent } from './navisworks-clashes-loading-screen.component';

describe('NavisworksClashesLoadingScreenComponent', () => {
  let component: NavisworksClashesLoadingScreenComponent;
  let fixture: ComponentFixture<NavisworksClashesLoadingScreenComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NavisworksClashesLoadingScreenComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(NavisworksClashesLoadingScreenComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
