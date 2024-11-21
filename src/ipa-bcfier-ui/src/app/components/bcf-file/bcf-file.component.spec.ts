import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BcfFileComponent } from './bcf-file.component';
import { AppTestingModule } from '../../app.testing.module';

describe('BcfFileComponent', () => {
  let component: BcfFileComponent;
  let fixture: ComponentFixture<BcfFileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BcfFileComponent, AppTestingModule]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BcfFileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
