import { ComponentFixture, TestBed } from '@angular/core/testing';

import { IssueFiltersComponent } from './issue-filters.component';
import { AppTestingModule } from '../../app.testing.module';

describe('IssueFiltersComponent', () => {
  let component: IssueFiltersComponent;
  let fixture: ComponentFixture<IssueFiltersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IssueFiltersComponent, AppTestingModule],
    }).compileComponents();

    fixture = TestBed.createComponent(IssueFiltersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
