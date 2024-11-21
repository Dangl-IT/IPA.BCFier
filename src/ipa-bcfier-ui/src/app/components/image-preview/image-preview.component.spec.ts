import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ImagePreviewComponent } from './image-preview.component';
import { AppTestingModule } from '../../app.testing.module';

describe('ImagePreviewComponent', () => {
  let component: ImagePreviewComponent;
  let fixture: ComponentFixture<ImagePreviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImagePreviewComponent, AppTestingModule]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ImagePreviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
