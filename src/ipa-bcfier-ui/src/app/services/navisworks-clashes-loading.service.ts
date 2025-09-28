import { MatDialog, MatDialogRef } from '@angular/material/dialog';

import { Injectable, inject } from '@angular/core';
import { NavisworksClashesLoadingScreenComponent } from '../components/navisworks-clashes-loading-screen/navisworks-clashes-loading-screen.component';

@Injectable({
  providedIn: 'root',
})
export class NavisworksClashesLoadingService {
  private matDialog = inject(MatDialog);

  private matDialogRef: MatDialogRef<NavisworksClashesLoadingScreenComponent> | null =
    null;

  public showLoadingScreen(): void {
    if (this.matDialogRef) {
      return;
    }

    this.matDialogRef = this.matDialog.open(
      NavisworksClashesLoadingScreenComponent,
      {
        disableClose: true,
      }
    );
  }

  public hideLoadingScreen(): void {
    if (!this.matDialogRef) {
      return;
    }

    this.matDialogRef.close();
    this.matDialogRef = null;
  }
}
