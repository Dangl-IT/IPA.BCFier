import { Component, EventEmitter, Input, Output } from '@angular/core';

import { BcfViewpointComponent } from '../../generated-client/generated-client';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'bcfier-elements-viewpoint',
  imports: [MatCardModule, MatListModule, MatIconModule, MatDividerModule],
  templateUrl: './elements-viewpoint.component.html',
  styleUrls: ['./elements-viewpoint.component.scss'],
  standalone: true,
})
export class ElementsViewpointComponent {
  @Input() viewpointElements: {
    name: string;
    components: BcfViewpointComponent[];
  }[] = [];
  @Output() selectedElement = new EventEmitter<string | null>();

  selectedAuthoringToolId: string | null = null;

  selectElement(component: BcfViewpointComponent): void {
    this.selectedAuthoringToolId = component.authoringToolId
      ? component.authoringToolId
      : null;
    this.selectedElement.emit(this.selectedAuthoringToolId);
  }
}
