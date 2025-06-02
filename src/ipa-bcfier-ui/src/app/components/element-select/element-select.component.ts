import { Component, input, output, signal } from '@angular/core';

import { IfcGuidNamePair } from '../../generated-client/generated-client';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'bcfier-element-select',
  imports: [MatFormFieldModule, MatSelectModule],
  templateUrl: './element-select.component.html',
  styleUrl: './element-select.component.scss',
})
export class ElementSelectComponent {
  label = input('Element');
  elements = input<IfcGuidNamePair[]>([]);
  selectedId = signal<string>('');
  selectedIdChange = output<string>();

  ngOnInit(): void {
    this.onSelectionChange('');
  }

  onSelectionChange(value: string) {
    this.selectedId.set(value);
    this.selectedIdChange.emit(value);
  }
}
