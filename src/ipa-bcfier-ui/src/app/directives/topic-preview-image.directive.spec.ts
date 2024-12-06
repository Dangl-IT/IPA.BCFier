import { ElementRef } from '@angular/core';
import { TopicPreviewImageDirective } from './topic-preview-image.directive';

describe('TopicPreviewImageDirective', () => {
  it('should create an instance', () => {
    const el = new ElementRef<HTMLDivElement>(document.createElement('div'));
    const directive = new TopicPreviewImageDirective(el);
    expect(directive).toBeTruthy();
  });
});
