import {
  Directive,
  effect,
  ElementRef,
  inject,
  input,
  Renderer2,
} from '@angular/core';

export interface TriangleCornerConfig {
  status: string;
  colors: Record<string, string>;
  zIndex?: number;
  triangleSize?: number;
}

@Directive({
  selector: '[bcfierTriangleCorner]',
})
export class TriangleCornerDirective {
  readonly config = input<TriangleCornerConfig | null>(null);

  private readonly el = inject(ElementRef);
  private readonly renderer = inject(Renderer2);
  private triangleEl: HTMLElement | null = null;
  private defaultzIndex = 10; // Default z-index
  private defaultTriangleSize = 30; // Default triangle size
  private defaultColor = 'black'; // Default color

  constructor() {
    effect(() => {
      const color =
        this.config()?.colors[this.config()?.status || ''] || this.defaultColor;
      const zIndex = this.config()?.zIndex ?? this.defaultzIndex;
      this.updateTriangle(color, zIndex);
    });
  }
  private updateTriangle(color: string, zIndex: number): void {
    if (this.triangleEl) {
      if (this.el.nativeElement.contains(this.triangleEl)) {
        this.renderer.removeChild(this.el.nativeElement, this.triangleEl);
      }
    }

    const triangle = this.renderer.createElement('div');
    const triangleSize =
      this.config()?.triangleSize || this.defaultTriangleSize;
    this.triangleEl = triangle;

    this.renderer.setStyle(triangle, 'position', 'absolute');
    this.renderer.setStyle(triangle, 'top', '0');
    this.renderer.setStyle(triangle, 'left', '0');
    this.renderer.setStyle(
      triangle,
      'border-top',
      `${triangleSize}px solid ${color}`
    );
    this.renderer.setStyle(
      triangle,
      'border-right',
      `${triangleSize}px solid transparent`
    );
    this.renderer.setStyle(triangle, 'z-index', zIndex.toString());

    this.renderer.setStyle(this.el.nativeElement, 'position', 'relative');
    this.renderer.appendChild(this.el.nativeElement, triangle);
  }
}
