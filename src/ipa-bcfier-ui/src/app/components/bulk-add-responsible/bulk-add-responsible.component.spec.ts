import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BulkAddResponsibleComponent } from './bulk-add-responsible.component';

describe('BulkAddResponsibleComponent', () => {
  let component: BulkAddResponsibleComponent;
  let fixture: ComponentFixture<BulkAddResponsibleComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BulkAddResponsibleComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(BulkAddResponsibleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
