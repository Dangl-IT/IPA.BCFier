import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, TemplateRef } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
@Component({
  selector: 'bcfier-confirm-dialog',
  imports: [MatButtonModule, MatDialogModule, NgTemplateOutlet],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmDialogComponent {
  data = inject<{
    cancelBtnText: string;
    action: string;
    contentTemplate: TemplateRef<any>;
}>(MAT_DIALOG_DATA);

  private dialogRef = inject(MatDialogRef<ConfirmDialogComponent>);

  close(confirm: boolean): void {
    this.dialogRef.close(confirm);
  }
}
