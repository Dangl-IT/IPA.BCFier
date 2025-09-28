import { ElementRef } from '@angular/core';
import { ViewpointImageDirective } from './viewpoint-image.directive';
import { TestBed } from '@angular/core/testing';

describe('ViewpointImageDirective', () => {
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
      const directive = new ViewpointImageDirective();
      expect(directive).toBeTruthy();
    })
  });
});
