import {
  createEnvironmentInjector,
  ElementRef,
  Renderer2,
  runInInjectionContext,
} from '@angular/core';
import { TriangleCornerDirective } from './triangle-corner.directive';
import { TestBed } from '@angular/core/testing';

describe('TriangleCornerDirective', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [
        {
          provide: ElementRef,
          useValue: {
            nativeElement: document.createElement('div'), // Custom mock value for nativeElement
          },
        },
        {
          provide: Renderer2,
          useValue: {},
        },
      ],
    }).compileComponents();
  });
  it('should create an instance', () => {
    TestBed.runInInjectionContext(() => {
      const directive = new TriangleCornerDirective();

      expect(directive).toBeTruthy();
    });
  });
});
