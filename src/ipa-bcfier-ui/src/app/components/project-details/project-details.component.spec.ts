import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProjectDetailsComponent } from './project-details.component';
import { AppTestingModule } from '../../app.testing.module';
import { ProjectGet } from '../../generated-client/generated-client';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

describe('ProjectDetailsComponent', () => {
  let component: ProjectDetailsComponent;
  let fixture: ComponentFixture<ProjectDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProjectDetailsComponent, AppTestingModule],
      providers: [
        {
          provide: MAT_DIALOG_DATA,
          useValue: { id: '1'} as ProjectGet,
        },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
