import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import {
  FormBuilder,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';

import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
    selector: 'bcfier-add-user',
    imports: [
        MatDialogModule,
        MatInputModule,
        MatButtonModule,
        MatFormFieldModule,
        ReactiveFormsModule,
        FormsModule,
    ],
    templateUrl: './add-user.component.html',
    styleUrl: './add-user.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddUserComponent {
  dialogRef = inject<MatDialogRef<AddUserComponent>>(MatDialogRef);
  private fb = inject(FormBuilder);

  newUserForm = this.fb.group({
    name: ['', Validators.required],
  });

  closeDialog(shouldSave: boolean): void {
    if (!shouldSave) {
      this.dialogRef.close();
      return;
    }

    this.dialogRef.close(this.newUserForm.value.name);
  }
}
