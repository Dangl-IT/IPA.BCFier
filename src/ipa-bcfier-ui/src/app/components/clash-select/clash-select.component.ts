import { Component, input, output, signal } from '@angular/core';
import { NavisworksClashSelection } from '../../generated-client/generated-client';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';

@Component({
  selector: 'bcfier-clash-select',
  imports: [MatFormFieldModule, MatSelectModule],
  templateUrl: './clash-select.component.html',
  styleUrls: ['./clash-select.component.scss'],
})
export class ClashSelectComponent {
  label = input('Clash Check');
  clashes = input<NavisworksClashSelection[]>([]);
  selectedIds = signal<string[]>([]);
  selectedIdsChange = output<string[]>();

  ngOnInit(): void {
    this.onSelectionChange([]);
  }

  onSelectionChange(value: string[]) {
    this.selectedIds.set(value);
    this.selectedIdsChange.emit(value);
  }
}
