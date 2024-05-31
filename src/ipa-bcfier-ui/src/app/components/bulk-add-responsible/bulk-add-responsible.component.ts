import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatSelectModule } from '@angular/material/select';
import { UsersService } from '../../services/users.service';
import { AsyncPipe } from '@angular/common';
import { ProjectUserGet } from '../../generated-client/generated-client';

@Component({
  selector: 'bcfier-bulk-add-responsible',
  standalone: true,
  imports: [MatButtonModule, MatDialogModule, MatSelectModule, AsyncPipe],
  templateUrl: './bulk-add-responsible.component.html',
  styleUrl: './bulk-add-responsible.component.scss',
})
export class BulkAddResponsibleComponent {
  private dialogRef = inject(MatDialogRef<BulkAddResponsibleComponent>);
  users$ = inject(UsersService).users;
  usersService = inject(UsersService);

  selectedUser: ProjectUserGet | null = null;

  refreshUsers(): void {
    this.usersService.refreshUsers();
  }

  save(): void {
    if (this.selectedUser) {
      this.dialogRef.close(this.selectedUser.identifier);
    }
  }

  close(): void {
    this.dialogRef.close();
  }
}
