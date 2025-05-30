import { Component, EventEmitter, Input, Output } from '@angular/core';

import { CommonModule } from '@angular/common';
import { IfcGuidNamePair } from '../../generated-client/generated-client';

@Component({
  selector: 'bcfier-elements-viewpoint',
  imports: [CommonModule],
  templateUrl: './elements-viewpoint.component.html',
  styleUrls: ['./elements-viewpoint.component.scss'],
})
export class ElementsViewpointComponent {
  @Input() viewpointElements: IfcGuidNamePair[] = [];
  @Output() selectedElement = new EventEmitter<IfcGuidNamePair>();

  selectedElementId: string | undefined;

  selectElement(element: IfcGuidNamePair): void {
    this.selectedElementId = element.ifcGuid;
    this.selectedElement.emit(element);
  }
}
