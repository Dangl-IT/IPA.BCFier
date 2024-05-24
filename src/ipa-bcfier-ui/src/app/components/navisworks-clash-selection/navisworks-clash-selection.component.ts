import { Component, inject } from '@angular/core';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import {
  NavisworksClashSelection,
  ViewpointsClient,
} from '../../generated-client/generated-client';

import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'bcfier-navisworks-clash-selection',
  standalone: true,
  imports: [MatInputModule, MatDialogModule, MatButtonModule, MatSelectModule],
  templateUrl: './navisworks-clash-selection.component.html',
  styleUrl: './navisworks-clash-selection.component.scss',
})
export class NavisworksClashSelectionComponent {
  viewpointsClient = inject(ViewpointsClient);
  constructor(
    public dialogRef: MatDialogRef<NavisworksClashSelectionComponent>
  ) {
    this.viewpointsClient
      .getAvailableNavisworksClashes()
      .subscribe((clashes) => {
        this.clashes = clashes;
      });
  }

  clashes: NavisworksClashSelection[] = [];
  selectedClashId: string | null = null;

  save(): void {
    this.dialogRef.close(this.selectedClashId);
  }

  close(): void {
    this.dialogRef.close();
  }
}
