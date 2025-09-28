import { Directive, ElementRef, Input, SimpleChanges, inject } from '@angular/core';

import { BcfViewpoint } from '../generated-client/generated-client';

@Directive({
  selector: '[bcfierViewpointImage]',
  standalone: true,
})
export class ViewpointImageDirective {
  private elementRef = inject(ElementRef);

  @Input() bcfierViewpointImage: BcfViewpoint | null = null;

  ngOnInit(): void {
    this.handleImage();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['bcfierViewpointImage']) {
      this.handleImage();
    }
  }

  private handleImage(): void {
    if (!this.bcfierViewpointImage?.snapshotBase64) {
      this.elementRef.nativeElement.style.display = 'none';
      return;
    }

    this.elementRef.nativeElement.style.display = null;

    // We're using the base64 data from the snapshot to
    // generate a base64 data url with png type
    const imageUrl = `data:image/png;base64,${this.bcfierViewpointImage.snapshotBase64}`;
    // Then we're appending it to the host element as src attribute
    this.elementRef.nativeElement.src = imageUrl;
  }
}
