import {
  BcfFile,
  BcfProjectExtensions,
  BcfTopic,
} from '../../../generated/models';
import { Component, Input, OnInit, inject } from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { AddStringValueComponent } from '../add-string-value/add-string-value.component';
import { BackendService } from '../../services/BackendService';
import { CommentsDetailComponent } from '../comments-detail/comments-detail.component';
import { CommentsViewpointFilterPipe } from '../../pipes/comments-viewpoint-filter.pipe';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { IssueStatusesService } from '../../services/issue-statuses.service';
import { IssueTypesService } from '../../services/issue-types.service';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';
import { UsersService } from '../../services/users.service';

@Component({
  selector: 'bcfier-topic-detail',
  standalone: true,
  imports: [
    FormsModule,
    MatIconModule,
    MatCardModule,
    MatButtonModule,
    MatInputModule,
    CommonModule,
    MatSelectModule,
    MatDialogModule,
    AddStringValueComponent,
    CommentsViewpointFilterPipe,
    CommentsDetailComponent,
    MatDatepickerModule,
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './topic-detail.component.html',
  styleUrl: './topic-detail.component.scss',
})
export class TopicDetailComponent implements OnInit {
  @Input() topic!: BcfTopic;
  @Input() bcfFile!: BcfFile;
  issueStatusesService = inject(IssueStatusesService);
  issueTypesService = inject(IssueTypesService);
  users$ = inject(UsersService).users;
  extensions!: BcfProjectExtensions;
  issueStatuses$ = this.issueStatusesService.issueStatuses;
  issueTypes$ = this.issueTypesService.issueTypes;

  constructor(
    private matDialog: MatDialog,
    private backendService: BackendService
  ) {}

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
          this.topic.topicType = result;
        }
      });
  }

  addViewpoint(): void {
    this.backendService.addViewpoint().subscribe((viewpoint) => {
      if (viewpoint) {
        this.topic.viewpoints.push(viewpoint);
      }
    });
  }
}
