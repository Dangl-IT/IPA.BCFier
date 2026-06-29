import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatInputModule } from '@angular/material/input';

@Component({
    selector: 'bcfier-add-string-value',
    imports: [MatInputModule, FormsModule, MatDialogModule, MatButtonModule],
    templateUrl: './add-string-value.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './add-string-value.component.scss'
})
export class AddStringValueComponent {
  dialogRef = inject<MatDialogRef<AddStringValueComponent>>(MatDialogRef);
  data = inject<{
    header: string;
}>(MAT_DIALOG_DATA);

  value = '';

  close(shouldSave: boolean): void {
    if (shouldSave) {
      this.dialogRef.close(this.value);
    } else {
      this.dialogRef.close();
    }
  }
}
