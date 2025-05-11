import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatSelectModule } from '@angular/material/select';
import { ProjectUsersService } from '../../services/project-users.service';
import { AsyncPipe } from '@angular/common';
import { ProjectUserGet } from '../../generated-client/generated-client';
import { IssueStatusesService } from '../../services/issue-statuses.service';
import { IssueTypesService } from '../../services/issue-types.service';
import { map } from 'rxjs';
import {FormsModule} from '@angular/forms';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MatTooltipModule} from '@angular/material/tooltip';

@Component({
  selector: 'bcfier-bulk-edit-topic',
  standalone: true,
  imports: [MatButtonModule, MatDialogModule, MatSelectModule, AsyncPipe, MatCheckboxModule, FormsModule, MatTooltipModule],
  templateUrl: './bulk-edit-topic.component.html',
  styleUrl: './bulk-edit-topic.component.scss',
})
export class BulkTopicEditComponent {
  private dialogRef = inject(MatDialogRef<BulkTopicEditComponent>);
  users$ = inject(ProjectUsersService).users.pipe(
    map((users) => [{ id: '', identifier: '' }, ...users])
  );
  issueStatusesService = inject(IssueStatusesService);
  issueTypesService = inject(IssueTypesService);
  projectUsersService = inject(ProjectUsersService);

  issueStatuses$ = this.issueStatusesService.issueStatuses.pipe(
    map((stati) => new Set<string | null>(['', ...stati]))
  );
  issueTypes$ = this.issueTypesService.issueTypes.pipe(
    map((types) => new Set<string | null>(['', ...types]))
  );

  selectedUser: ProjectUserGet[] = [];
  selectedType: string[] = [];
  selectedStatus: string | null = null;
  additionalMode = false;

  refreshUsers(): void {
    this.projectUsersService.refreshUsers();
  }

  save(): void {
    if (this.selectedUser.length || this.selectedType.length || this.selectedStatus) {
      this.dialogRef.close({
        responsibleUser: this.selectedUser.map(user => user.identifier),
        type: this.selectedType,
        status: this.selectedStatus,
        additionalMode: this.additionalMode,
      });
    }
  }

  close(): void {
    this.dialogRef.close();
  }

  changeUser(user: ProjectUserGet): void {
    const count = this.selectedUser.length;
    if (count > 0) {
      if (user.id === '') {
        if (count > 1) {
          for(let i = 0; i < count - 1; i++) {
            this.selectedUser.pop();
          }
        }
      } else {
        const indexNull = this.selectedUser.findIndex(user => user.id === '');
        if (indexNull !== -1) {
          this.selectedUser.splice(indexNull, 1);
        }
      }
    }
  }

  changeType(type: string | null): void {
    const count = this.selectedType.length;
    if (count > 0) {
      if (type === '') {
        if (count > 1) {
          for(let i = 0; i < count - 1; i++) {
            this.selectedType.pop();
          }
        }
      } else {
        const indexNull = this.selectedType.findIndex(item => item === '');
        if (indexNull !== -1) {
          this.selectedType.splice(indexNull, 1);
        }
      }
    }
  }

  clearSelecting(): void {
    this.selectedUser = [];
    this.selectedType = [];
    this.selectedStatus = null;
  }
}
