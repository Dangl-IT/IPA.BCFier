import {
  BcfFile,
  BcfProjectExtensions,
  BcfTopic,
  ViewpointsClient,
} from '../../generated-client/generated-client';
import { Component, Input, OnInit, inject, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import {
  MessageType,
  TeamsMessengerService,
} from '../../services/teams-messenger.service';
import { of, switchMap } from 'rxjs';

import { AddStringValueComponent } from '../add-string-value/add-string-value.component';
import { BackendService } from '../../services/BackendService';
import { BcfFileAutomaticallySaveService } from '../../services/bcf-file-automaticaly-save.service';
import { CommentsDetailComponent } from '../comments-detail/comments-detail.component';
import { CommentsViewpointFilterPipe } from '../../pipes/comments-viewpoint-filter.pipe';
import { CommonModule } from '@angular/common';
import { IssueStatusesService } from '../../services/issue-statuses.service';
import { IssueTypesService } from '../../services/issue-types.service';
import { LoadingService } from '../../services/loading.service';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { NotificationsService } from '../../services/notifications.service';
import { ProjectUsersService } from '../../services/project-users.service';

@Component({
  selector: 'bcfier-topic-detail',
  imports: [
    FormsModule,
    MatIconModule,
    MatCardModule,
    MatButtonModule,
    MatInputModule,
    CommonModule,
    MatSelectModule,
    MatDialogModule,
    CommentsViewpointFilterPipe,
    CommentsDetailComponent,
    MatDatepickerModule,
  ],
  templateUrl: './topic-detail.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './topic-detail.component.scss',
})
export class TopicDetailComponent implements OnInit {
  private matDialog = inject(MatDialog);
  private backendService = inject(BackendService);

  @Input() topic!: BcfTopic;
  @Input() bcfFile!: BcfFile;
  issueStatusesService = inject(IssueStatusesService);
  issueTypesService = inject(IssueTypesService);
  users$ = inject(ProjectUsersService).users;
  bcfFileAutomaticallySaveService = inject(BcfFileAutomaticallySaveService);
  teamsMessengerService = inject(TeamsMessengerService);
  extensions!: BcfProjectExtensions;
  issueStatuses$ = this.issueStatusesService.issueStatuses;
  issueTypes$ = this.issueTypesService.issueTypes;
  isTitleChangeFirstTime = false;
  defaultTopicTitle = 'New Issue';
  projectUsersService = inject(ProjectUsersService);
  private viewpointsClient = inject(ViewpointsClient);
  private notificationsService = inject(NotificationsService);
  private loadingService = inject(LoadingService);

  ngOnInit(): void {
    if (this.bcfFile?.projectExtensions?.topicStatuses) {
      this.issueStatusesService.setIssueStatuses(
        this.bcfFile?.projectExtensions?.topicStatuses
      );
    }

    if (this.bcfFile?.projectExtensions?.topicTypes) {
      this.issueTypesService.setIssueTypes(
        this.bcfFile?.projectExtensions?.topicTypes
      );
    }
    this.extensions = this.bcfFile?.projectExtensions || {
      priorities: [],
      snippetTypes: [],
      topicLabels: [],
      topicStatuses: ['Open', 'Closed', 'InProgress', 'ReOpened'],
      topicTypes: [
        'Info',
        'Issue',
        'Error',
        'Comment',
        'Request',
        'Structural',
      ],
      users: [],
    };
  }

  addNewStatus(): void {
    this.matDialog
      .open(AddStringValueComponent, {
        data: {
          header: 'Status',
        },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result) {
          this.extensions.topicStatuses.push(result);
          this.topic.topicStatus = result;
        }
      });
  }

  addNewType(): void {
    this.matDialog
      .open(AddStringValueComponent, {
        data: {
          header: 'Topic Type',
        },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result) {
          this.extensions.topicTypes.push(result);
          this.topic.topicTypes = result;
        }
      });
  }

  addViewpoint(): void {
    this.backendService.addViewpoint().subscribe((viewpoint) => {
      if (viewpoint) {
        this.topic.viewpoints = [...this.topic.viewpoints, viewpoint];
        this.teamsMessengerService.sendMessageToTeams(MessageType.AddViewpoint);
        this.bcfFileAutomaticallySaveService.saveCurrentActiveBcfFileAutomatically();
      }
    });
  }

  changeIssue(): void {
    this.bcfFileAutomaticallySaveService.saveCurrentActiveBcfFileAutomatically();
    if (this.isTitleChangeFirstTime) {
      this.teamsMessengerService.sendMessageToTeams(MessageType.ChangeTitle);
    }
    this.isTitleChangeFirstTime = false;
  }

  checkIsTitleChangeFirstTime(e: string = ''): void {
    if (e === this.defaultTopicTitle) {
      this.isTitleChangeFirstTime = true;
    }
  }

  refreshUsers(): void {
    this.projectUsersService.refreshUsers();
  }
}
