import { MatDialog, MatDialogRef } from '@angular/material/dialog';

import { Injectable } from '@angular/core';
import { NavisworksClashesLoadingScreenComponent } from '../components/navisworks-clashes-loading-screen/navisworks-clashes-loading-screen.component';

@Injectable({
  providedIn: 'root',
})
export class NavisworksClashesLoadingService {
  private matDialogRef: MatDialogRef<NavisworksClashesLoadingScreenComponent> | null =
    null;

  constructor(private matDialog: MatDialog) {}

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
