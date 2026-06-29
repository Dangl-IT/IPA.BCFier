import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';

@Component({
    selector: 'bcfier-cad-error-dialog',
    imports: [MatButtonModule, MatDialogModule],
    templateUrl: './cad-error-dialog.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './cad-error-dialog.component.scss'
})
export class CadErrorDialogComponent {
  dialogRef = inject<MatDialogRef<CadErrorDialogComponent>>(MatDialogRef);
  errorMessage = inject<string>(MAT_DIALOG_DATA);


  close(): void {
    this.dialogRef.close();
  }
}
