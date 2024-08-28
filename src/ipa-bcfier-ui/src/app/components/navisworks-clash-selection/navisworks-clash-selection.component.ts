import { Component, inject } from '@angular/core';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import {
  NavisworksClashSelection,
  ViewpointsClient,
} from '../../generated-client/generated-client';

import { FormsModule } from '@angular/forms';
import { LoadingService } from '../../services/loading.service';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'bcfier-navisworks-clash-selection',
  standalone: true,
  imports: [
    MatInputModule,
    MatDialogModule,
    MatButtonModule,
    MatSelectModule,
    MatCheckboxModule,
    FormsModule,
    MatTooltipModule,
  ],
  templateUrl: './navisworks-clash-selection.component.html',
  styleUrl: './navisworks-clash-selection.component.scss',
})
export class NavisworksClashSelectionComponent {
  viewpointsClient = inject(ViewpointsClient);
  loadingService = inject(LoadingService);
  onlyImportNew = false;
  constructor(
    public dialogRef: MatDialogRef<NavisworksClashSelectionComponent>
  ) {
    this.loadingService.showLoadingScreen();
    this.viewpointsClient.getAvailableNavisworksClashes().subscribe({
      next: (clashes) => {
        this.loadingService.hideLoadingScreen();
        this.clashes = clashes;
      },
      error: () => {
        this.loadingService.hideLoadingScreen();
      },
    });
  }

  clashes: NavisworksClashSelection[] = [];
  selectedClashId: string | null = null;
  selectedStatusType: string | null = null;

  save(): void {
    this.dialogRef.close({
      clashId: this.selectedClashId,
      onlyImportNew: this.onlyImportNew,
      statusType: this.selectedStatusType,
    });
  }

  close(): void {
    this.dialogRef.close();
  }
}
