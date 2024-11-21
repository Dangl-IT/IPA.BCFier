import { ElementRef } from '@angular/core';
import { ViewpointImageDirective } from './viewpoint-image.directive';

describe('ViewpointImageDirective', () => {
  it('should create an instance', () => {
    const el = new ElementRef<HTMLDivElement>(document.createElement('div'));
    const directive = new ViewpointImageDirective(el);
    expect(directive).toBeTruthy();
  });
});
