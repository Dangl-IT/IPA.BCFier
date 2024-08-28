import { Component, Inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';

@Component({
  selector: 'bcfier-cad-error-dialog',
  standalone: true,
  imports: [MatButtonModule, MatDialogModule],
  templateUrl: './cad-error-dialog.component.html',
  styleUrl: './cad-error-dialog.component.scss',
})
export class CadErrorDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<CadErrorDialogComponent>,
    @Inject(MAT_DIALOG_DATA)
    public errorMessage: string
  ) {}

  close(): void {
    this.dialogRef.close();
  }
}
