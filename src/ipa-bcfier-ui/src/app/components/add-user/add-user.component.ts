import { ChangeDetectionStrategy, Component } from '@angular/core';
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
  standalone: true,
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
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddUserComponent {
  newUserForm = this.fb.group({
    name: ['', Validators.required],
  });
  constructor(
    public dialogRef: MatDialogRef<AddUserComponent>,
    private fb: FormBuilder
  ) {}

  closeDialog(shouldSave: boolean): void {
    if (!shouldSave) {
      this.dialogRef.close();
      return;
    }

    this.dialogRef.close(this.newUserForm.value.name);
  }
}
