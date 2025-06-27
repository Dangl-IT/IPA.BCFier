import { Component, inject, Input } from '@angular/core';
import {
  BcfTopic,
  ProjectUserGet,
} from '../../generated-client/generated-client';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { ProjectUsersService } from '../../services/project-users.service';
import { map } from 'rxjs';
import { IssueStatusesService } from '../../services/issue-statuses.service';
import { IssueTypesService } from '../../services/issue-types.service';
import { CommonModule } from '@angular/common';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { BcfFileAutomaticallySaveService } from '../../services/bcf-file-automaticaly-save.service';

@Component({
  selector: 'bcfier-selected-edit-topic',
  imports: [
    CommonModule,
    FormsModule,
    MatInputModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatDatepickerModule,
    MatCheckboxModule,
    MatButtonModule,
  ],
  templateUrl: './selected-edit-topic.component.html',
  styleUrl: './selected-edit-topic.component.scss',
})
export class SelectedEditTopicComponent {
  @Input() selectedListTopic: BcfTopic[] = [];
  users$ = inject(ProjectUsersService).users.pipe(
    map((users) => [{ id: '', identifier: '' }, ...users])
  );
  issueStatusesService = inject(IssueStatusesService);
  issueTypesService = inject(IssueTypesService);
  projectUsersService = inject(ProjectUsersService);
  bcfFileAutomaticallySaveService = inject(BcfFileAutomaticallySaveService);

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
  selectedDueDate: Date | null = null;

  ngOnInit() {
    console.log(this.selectedListTopic);
  }

  refreshUsers(): void {
    this.projectUsersService.refreshUsers();
  }

  getListTopicTitles(selectedListTopic: BcfTopic[]): string[] {
    return selectedListTopic.map((topic) => topic.title || 'No Title');
  }

  changeUser(user: ProjectUserGet): void {
    const count = this.selectedUser.length;
    if (count > 0) {
      if (user.id === '') {
        if (count > 1) {
          for (let i = 0; i < count - 1; i++) {
            this.selectedUser.pop();
          }
        }
      } else {
        const indexNull = this.selectedUser.findIndex((user) => user.id === '');
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
          for (let i = 0; i < count - 1; i++) {
            this.selectedType.pop();
          }
        }
      } else {
        const indexNull = this.selectedType.findIndex((item) => item === '');
        if (indexNull !== -1) {
          this.selectedType.splice(indexNull, 1);
        }
      }
    }
  }

  changeAdditionalMode(value: boolean): void {
    this.additionalMode = value;
    this.clearSelecting();
  }

  clearSelecting(): void {
    this.selectedUser = [];
    this.selectedType = [];
    this.selectedStatus = null;
    this.selectedDueDate = null;
  }

  save(): void {
    if (
      this.selectedUser.length ||
      this.selectedType.length ||
      this.selectedStatus !== null ||
      this.selectedDueDate
    ) {
      const bulkOptions = {
        responsibleUser: this.selectedUser.map((user) => user.identifier),
        type: this.selectedType,
        status: this.selectedStatus,
        dueDate: this.selectedDueDate,
        additionalMode: this.additionalMode,
      };
      this.selectedListTopic.forEach((topic) => {
        if (bulkOptions.status !== null) {
          topic.topicStatus = bulkOptions.status;
        }
        if (bulkOptions.type.length) {
          if (bulkOptions.additionalMode) {
            bulkOptions.type.forEach((type) => {
              if (!topic.topicTypes?.includes(type)) {
                topic.topicTypes = [...(topic.topicTypes || []), type];
              }
            });
          } else {
            topic.topicTypes =
              bulkOptions.type[0] === '' ? [] : bulkOptions.type;
          }
        }
        if (bulkOptions.responsibleUser.length) {
          if (bulkOptions.additionalMode) {
            bulkOptions.responsibleUser.forEach((user) => {
              if (!topic.assignedToList?.includes(user)) {
                topic.assignedToList = [...(topic.assignedToList || []), user];
              }
            });
          } else {
            topic.assignedToList =
              bulkOptions.responsibleUser[0] === ''
                ? []
                : bulkOptions.responsibleUser;
          }
        }
        if (bulkOptions.dueDate) {
          topic.dueDate = bulkOptions.dueDate;
        }
      });

      this.bcfFileAutomaticallySaveService.saveCurrentActiveBcfFileAutomatically();

      this.cancel();
    }
  }

  cancel(): void {
    this.clearSelecting();
    this.additionalMode = false;
  }
}
