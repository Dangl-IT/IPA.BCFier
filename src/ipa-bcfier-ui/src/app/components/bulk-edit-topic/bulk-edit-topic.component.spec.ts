import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BulkTopicEditComponent } from './bulk-edit-topic.component';

describe('BulkTopicEditComponent', () => {
  let component: BulkTopicEditComponent;
  let fixture: ComponentFixture<BulkTopicEditComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BulkTopicEditComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(BulkTopicEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
