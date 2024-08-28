import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  Inject,
  inject,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
export type IConfirmdialogAction = 'delete';
@Component({
  selector: 'bcfier-confirm-dialog',
  standalone: true,
  imports: [MatButtonModule, MatDialogModule],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmDialogComponent {
  action: string;
  private dialogRef = inject(MatDialogRef<ConfirmDialogComponent>);
  constructor(
    @Inject(MAT_DIALOG_DATA)
    public data: IConfirmdialogAction
  ) {
    this.action = data;
  }

  close(confirm: boolean): void {
    this.dialogRef.close(confirm);
  }
}
