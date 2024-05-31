import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatSelectModule } from '@angular/material/select';
import { ProjectUsersService } from '../../services/project-users.service';
import { AsyncPipe } from '@angular/common';
import { ProjectUserGet } from '../../generated-client/generated-client';
import { IssueStatusesService } from '../../services/issue-statuses.service';
import { IssueTypesService } from '../../services/issue-types.service';

@Component({
  selector: 'bcfier-bulk-edit-topic',
  standalone: true,
  imports: [MatButtonModule, MatDialogModule, MatSelectModule, AsyncPipe],
  templateUrl: './bulk-edit-topic.component.html',
  styleUrl: './bulk-edit-topic.component.scss',
})
export class BulkTopicEditComponent {
  private dialogRef = inject(MatDialogRef<BulkTopicEditComponent>);
  users$ = inject(ProjectUsersService).users;
  issueStatusesService = inject(IssueStatusesService);
  issueTypesService = inject(IssueTypesService);
  projectUsersService = inject(ProjectUsersService);

  issueStatuses$ = this.issueStatusesService.issueStatuses;
  issueTypes$ = this.issueTypesService.issueTypes;

  selectedUser: ProjectUserGet | null = null;
  selectedType: string | null = null;
  selectedStatus: string | null = null;

  refreshUsers(): void {
    this.projectUsersService.refreshUsers();
  }

  save(): void {
    if (this.selectedUser || this.selectedType || this.selectedStatus) {
      this.dialogRef.close({
        responsibleUser: this.selectedUser?.identifier || null,
        type: this.selectedType,
        status: this.selectedStatus,
      });
    }
  }

  close(): void {
    this.dialogRef.close();
  }
}
