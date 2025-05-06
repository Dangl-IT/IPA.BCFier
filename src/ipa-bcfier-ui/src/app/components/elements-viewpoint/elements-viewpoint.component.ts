import { Component, EventEmitter, Input,Output } from '@angular/core';
import { CommonModule } from '@angular/common';

export type elementClash = {
  id: string;
  name: string;
};

@Component({
  selector: 'bcfier-elements-viewpoint',
  imports: [CommonModule],
  templateUrl: './elements-viewpoint.component.html',
  styleUrl: './elements-viewpoint.component.scss',
})
export class ElementsViewpointComponent {
  @Input() viewpointElements: elementClash[] = [];
  @Output() selectedElement = new EventEmitter<elementClash>();

  selectedElementId: string | null = null;

  selectElement(element: elementClash): void {
    this.selectedElementId = element.id;
    this.selectedElement.emit(element);
  }
}
