import { BcfFile, BcfTopic } from '../../generated-client/generated-client';
import { ChangeDetectorRef, Component, Input, inject } from '@angular/core';
import { FormGroup, FormsModule } from '@angular/forms';
import {
  IFilters,
  IssueFiltersComponent,
} from '../issue-filters/issue-filters.component';

import { BcfFileAutomaticallySaveService } from '../../services/bcf-file-automaticaly-save.service';
import { CommonModule } from '@angular/common';
import { IssueFilterService } from '../../services/issue-filter.service';
import { IssueStatusesService } from '../../services/issue-statuses.service';
import { IssueTypesService } from '../../services/issue-types.service';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { SafeUrlPipe } from '../../pipes/safe-url.pipe';
import { TopicDetailComponent } from '../topic-detail/topic-detail.component';
import { TopicFilterPipe } from '../../pipes/topic-filter.pipe';
import { TopicPreviewImageDirective } from '../../directives/topic-preview-image.directive';
import { UsersService } from '../../services/users.service';
import { getNewRandomGuid } from '../../functions/uuid';
import { TeamsMessengerService } from '../../services/teams-messenger.service';
import { TopicMessengerService } from '../../services/topic-messenger.service';
import { SettingsMessengerService } from '../../services/settings-messenger.service';
import { take } from 'rxjs';

@Component({
  selector: 'bcfier-bcf-file',
  standalone: true,
  imports: [
    MatButtonModule,
    MatInputModule,
    CommonModule,
    MatCardModule,
    MatIconModule,
    TopicPreviewImageDirective,
    FormsModule,
    TopicFilterPipe,
    MatProgressBarModule,
    TopicDetailComponent,
    MatSidenavModule,
    IssueFiltersComponent,
    SafeUrlPipe,
  ],
  templateUrl: './bcf-file.component.html',
  styleUrl: './bcf-file.component.scss',
})
export class BcfFileComponent {
  @Input() bcfFile!: BcfFile;
  issueStatuses$ = inject(IssueStatusesService).issueStatuses;
  issueTypes$ = inject(IssueTypesService).issueTypes;
  users$ = inject(UsersService).users;
  issueFilterService = inject(IssueFilterService);
  bcfFileAutomaticallySaveService = inject(BcfFileAutomaticallySaveService);
  teamsMessengerService = inject(TeamsMessengerService);
  topicMessengerService = inject(TopicMessengerService);
  settingsMessengerService = inject(SettingsMessengerService);
  cdr = inject(ChangeDetectorRef);
  selectedTopic: BcfTopic | null = null;
  filtredTopics: BcfTopic[] = [];

  ngOnInit() {
    this.selectedTopic = this.bcfFile.topics[0] || null;
    this.topicMessengerService.setSelectedTopic(this.selectedTopic);
    this.cdr.detectChanges();
    this.filtredTopics = [...this.bcfFile.topics];
  }

  private _search = '';
  public set search(value: string) {
    this._search = value;
  }
  public get search(): string {
    return this._search;
  }

  selectTopic(topic: BcfTopic) {
    this.selectedTopic = topic;
    this.topicMessengerService.setSelectedTopic(this.selectedTopic);
  }

  addIssue(): void {
    this.settingsMessengerService.settings.pipe(take(1)).subscribe((s) => {
      const newIssue: BcfTopic = {
        comments: [],
        id: getNewRandomGuid(),
        files: [],
        labels: [],
        referenceLinks: [],
        documentReferences: [],
        relatedTopicIds: [],
        viewpoints: [],
        assignedTo: '',
        creationAuthor: s.username,
        description: '',
        priority: '',
        title: 'New Issue',
        topicStatus: '',
        stage: '',
        topicType: '',
        serverAssignedId: '',
        modifiedAuthor: '',
        creationDate: new Date(),
      };
      this.bcfFile.topics.push(newIssue);
      this.selectedTopic = newIssue;
      this.topicMessengerService.setSelectedTopic(this.selectedTopic);
      this.filtredTopics = [...this.bcfFile.topics];
      this.bcfFileAutomaticallySaveService.saveCurrentActiveBcfFileAutomatically();
    });
  }

  removeIssue(): void {
    if (!this.selectedTopic) {
      return;
    }

    this.bcfFile.topics = this.bcfFile.topics.filter(
      (topic) => topic.id !== this.selectedTopic?.id
    );

    if (this.bcfFile.topics.length > 0) {
      this.selectedTopic = this.bcfFile.topics[0];
    } else {
      this.selectedTopic = null;
    }
    this.topicMessengerService.setSelectedTopic(this.selectedTopic);
    this.filtredTopics = [...this.bcfFile.topics];
    this.bcfFileAutomaticallySaveService.saveCurrentActiveBcfFileAutomatically();
  }

  filterIssues(filters: FormGroup<IFilters>): void {
    const { status, type, users, issueRange } = filters.value;
    if (
      status === undefined ||
      type === undefined ||
      users === undefined ||
      issueRange === undefined ||
      issueRange?.start === undefined ||
      issueRange?.end === undefined
    ) {
      return;
    }

    const isValuePresentInFilters =
      !!status || !!type || !!users || !!issueRange.start || !!issueRange.end;

    this.filtredTopics = isValuePresentInFilters
      ? [
          ...this.issueFilterService.filterIssue(
            this.bcfFile.topics,
            status,
            type,
            users,
            issueRange?.start,
            issueRange?.end
          ),
        ]
      : this.bcfFile.topics;
  }
}
