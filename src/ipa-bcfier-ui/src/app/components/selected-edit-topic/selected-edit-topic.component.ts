import {
  BcfTopic,
  ProjectUserGet,
} from '../../generated-client/generated-client';
import {
  Component,
  Input,
  NgZone,
  OnDestroy,
  OnInit,
  inject,
} from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Subject, map, takeUntil } from 'rxjs';

import { BcfFileAutomaticallySaveService } from '../../services/bcf-file-automaticaly-save.service';
import { CommonModule } from '@angular/common';
import { IssueStatusesService } from '../../services/issue-statuses.service';
import { IssueTypesService } from '../../services/issue-types.service';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { ProjectUsersService } from '../../services/project-users.service';
import { SelectedTopicListMessengerService } from '../../services/messengers/selected-topic-list.messenger.service';

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
export class SelectedEditTopicComponent implements OnInit, OnDestroy {
  @Input() selectedListTopic: BcfTopic[] = [];
  projectUsersService = inject(ProjectUsersService);
  private ngZone = inject(NgZone);
  users$ = this.projectUsersService.users.pipe(
    map((users) => {
      this.projectUsers = users;
      setTimeout(() => {
        this.ngZone.run(() => {
          this.calculateTopicsUserData();
        });
      }, 1);
      return [{ id: '', identifier: '' }, ...users];
    })
  );
  private projectUsers: ProjectUserGet[] = [];
  issueStatusesService = inject(IssueStatusesService);
  issueTypesService = inject(IssueTypesService);
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
  private _selectedDueDate: Date | null = null;
  get selectedDueDate(): Date | null {
    return this._selectedDueDate;
  }
  set selectedDueDate(value: Date | null) {
    this._selectedDueDate = value;
    this.save();
  }
  private $destroy = new Subject<void>();
  private selectedTopicListMessengerService = inject(
    SelectedTopicListMessengerService
  );

  ngOnInit(): void {
    this.refreshUsers();
    this.calculateTopicSelectionData();
    this.calculateTopicsUserData();

    this.selectedTopicListMessengerService.selectedTopicListChanged
      .pipe(takeUntil(this.$destroy))
      .subscribe(() => {
        this.calculateTopicSelectionData();
        this.calculateTopicsUserData();
      });
  }

  ngOnDestroy(): void {
    this.$destroy.next();
    this.$destroy.complete();
  }

  private calculateTopicSelectionData(): void {
    let firstStatus = this.selectedListTopic[0]?.topicStatus || null;
    for (const topic of this.selectedListTopic) {
      if (topic.topicStatus !== firstStatus) {
        firstStatus = null;
        break;
      }
    }
    this.selectedStatus = firstStatus;

    let firstDueDate = this.selectedListTopic[0]?.dueDate || null;
    for (const topic of this.selectedListTopic) {
      if (topic.dueDate !== firstDueDate) {
        firstDueDate = null;
        break;
      }
    }
    this._selectedDueDate = firstDueDate;

    const firstTypes = this.selectedListTopic[0]?.topicTypes || [];
    // We're checking for each topic if the types are the same
    // and no topic contains an empty type.
    const allTypesSame = this.selectedListTopic.every(
      (topic) =>
        topic.topicTypes &&
        topic.topicTypes.length > 0 &&
        topic.topicTypes.every((type) => firstTypes.includes(type))
    );
    if (allTypesSame) {
      this.selectedType = firstTypes;
    } else {
      // If not all types are the same, we just dont select any types
      this.selectedType = [];
    }
  }

  private calculateTopicsUserData(): void {
    // We're checking for each topic if the assignedToList is the same
    const firstAssignedToList = this.selectedListTopic[0]?.assignedToList || [];
    const allAssignedToSame = this.selectedListTopic.every(
      (topic) =>
        topic.assignedToList &&
        topic.assignedToList.length > 0 &&
        topic.assignedToList.every((userId) =>
          firstAssignedToList.includes(userId)
        )
    );
    if (allAssignedToSame) {
      let topicUsers = firstAssignedToList
        .map((userId) => this.projectUsers.find((u) => u.identifier == userId))
        .filter((user) => user !== undefined);
      // If the found users count is the same as the actual topic users count, we can safely assign them
      if (topicUsers.length === firstAssignedToList.length) {
        this.selectedUser = topicUsers;
      } else {
        this.selectedUser = [];
      }
    } else {
      // If not all assignedToList are the same, we just dont select any users
      this.selectedUser = [];
    }
  }

  refreshUsers(): void {
    this.projectUsersService.refreshUsers();
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
      };
      this.selectedListTopic.forEach((topic) => {
        if (bulkOptions.status !== null) {
          topic.topicStatus = bulkOptions.status;
        }
        if (bulkOptions.type.length) {
          topic.topicTypes = bulkOptions.type[0] === '' ? [] : bulkOptions.type;
        }
        if (bulkOptions.responsibleUser.length) {
          topic.assignedToList =
            bulkOptions.responsibleUser[0] === ''
              ? []
              : bulkOptions.responsibleUser;
        }
        if (bulkOptions.dueDate) {
          topic.dueDate = bulkOptions.dueDate;
        }
      });

      this.bcfFileAutomaticallySaveService.saveCurrentActiveBcfFileAutomatically();
    }
  }
}
