import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BulkTopicEditComponent } from './bulk-edit-topic.component';
import { AppTestingModule } from '../../app.testing.module';

describe('BulkTopicEditComponent', () => {
  let component: BulkTopicEditComponent;
  let fixture: ComponentFixture<BulkTopicEditComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BulkTopicEditComponent, AppTestingModule],
    }).compileComponents();

    fixture = TestBed.createComponent(BulkTopicEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
