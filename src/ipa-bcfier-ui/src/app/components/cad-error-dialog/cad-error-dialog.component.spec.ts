import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CadErrorDialogComponent } from './cad-error-dialog.component';

describe('CadErrorDialogComponent', () => {
  let component: CadErrorDialogComponent;
  let fixture: ComponentFixture<CadErrorDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CadErrorDialogComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(CadErrorDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
