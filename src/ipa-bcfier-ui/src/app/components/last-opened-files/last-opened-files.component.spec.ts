import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LastOpenedFilesComponent } from './last-opened-files.component';
import { AppTestingModule } from '../../app.testing.module';

describe('LastOpenedFilesComponent', () => {
  let component: LastOpenedFilesComponent;
  let fixture: ComponentFixture<LastOpenedFilesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LastOpenedFilesComponent, AppTestingModule],
    }).compileComponents();

    fixture = TestBed.createComponent(LastOpenedFilesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
