import { ElementRef } from '@angular/core';
import { TopicPreviewImageDirective } from './topic-preview-image.directive';
import { TestBed } from '@angular/core/testing';

describe('TopicPreviewImageDirective', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [
        {
          provide: ElementRef,
          useValue: {}
        },
      ],
    }).compileComponents();
  });
  
  it('should create an instance', () => {
    TestBed.runInInjectionContext(() => {
      const directive = new TopicPreviewImageDirective();
      expect(directive).toBeTruthy();
    });
  });
});
