import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SelectedEditTopicComponent } from './selected-edit-topic.component';

import { AppTestingModule } from '../../app.testing.module';

describe('SelectedEditTopicComponent', () => {
  let component: SelectedEditTopicComponent;
  let fixture: ComponentFixture<SelectedEditTopicComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SelectedEditTopicComponent, AppTestingModule],
    }).compileComponents();

    fixture = TestBed.createComponent(SelectedEditTopicComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
