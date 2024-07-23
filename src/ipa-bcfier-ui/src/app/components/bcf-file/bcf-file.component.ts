import {
  BcfFile,
  BcfTopic,
  ViewpointsClient,
} from '../../generated-client/generated-client';
import { ChangeDetectorRef, Component, Input, inject } from '@angular/core';
import { FormGroup, FormsModule } from '@angular/forms';
import {
  IFilters,
  IssueFiltersComponent,
} from '../issue-filters/issue-filters.component';
import {
  MessageType,
  TeamsMessengerService,
} from '../../services/teams-messenger.service';

import { AppConfigService } from '../../services/AppConfigService';
import { BcfFileAutomaticallySaveService } from '../../services/bcf-file-automaticaly-save.service';
import { BulkTopicEditComponent } from '../bulk-edit-topic/bulk-edit-topic.component';
import { CommonModule } from '@angular/common';
import { IssueFilterService } from '../../services/issue-filter.service';
import { IssueStatusesService } from '../../services/issue-statuses.service';
import { IssueTypesService } from '../../services/issue-types.service';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { NavisworksClashProgressMessengerService } from '../../services/messengers/navisworks-clash-progress-messenger.service';
import { NavisworksClashSelectionComponent } from '../navisworks-clash-selection/navisworks-clash-selection.component';
import { NavisworksClashesLoadingService } from '../../services/navisworks-clashes-loading.service';
import { NotificationsService } from '../../services/notifications.service';
import { ProjectUsersService } from '../../services/project-users.service';
import { SafeUrlPipe } from '../../pipes/safe-url.pipe';
import { SettingsMessengerService } from '../../services/settings-messenger.service';
import { TopicDetailComponent } from '../topic-detail/topic-detail.component';
import { TopicFilterPipe } from '../../pipes/topic-filter.pipe';
import { TopicMessengerService } from '../../services/topic-messenger.service';
import { TopicPreviewImageDirective } from '../../directives/topic-preview-image.directive';
import { getNewRandomGuid } from '../../functions/uuid';
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
  users$ = inject(ProjectUsersService).users;
  issueFilterService = inject(IssueFilterService);
  bcfFileAutomaticallySaveService = inject(BcfFileAutomaticallySaveService);
  teamsMessengerService = inject(TeamsMessengerService);
  topicMessengerService = inject(TopicMessengerService);
  settingsMessengerService = inject(SettingsMessengerService);
  navisworksClashProgressMessengerService = inject(
    NavisworksClashProgressMessengerService
  );
  cdr = inject(ChangeDetectorRef);
  selectedTopic: BcfTopic | null = null;
  filteredTopics: BcfTopic[] = [];
  isInNavisworks =
    inject(AppConfigService).getFrontendConfig().isConnectedToNavisworks;
  viewpointsClient = inject(ViewpointsClient);
  navisworksClashesLoadingService = inject(NavisworksClashesLoadingService);
  notificationsService = inject(NotificationsService);
  private dialog = inject(MatDialog);

  ngOnInit() {
    this.selectedTopic = this.bcfFile.topics[0] || null;
    this.topicMessengerService.setSelectedTopic(this.selectedTopic);
    this.cdr.detectChanges();
    this.filteredTopics = [...this.bcfFile.topics];
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
      this.filteredTopics = [...this.bcfFile.topics];
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
    this.filteredTopics = [...this.bcfFile.topics];
    this.bcfFileAutomaticallySaveService.saveCurrentActiveBcfFileAutomatically();
  }

  filterIssues(filters: FormGroup<IFilters>): void {
    const {
      withoutStatus,
      status,
      withoutType,
      type,
      withoutUser,
      users,
      issueRange,
    } = filters.value;
    if (
      withoutStatus === undefined ||
      (!withoutStatus && status === undefined) ||
      withoutType === undefined ||
      (!withoutType && type === undefined) ||
      withoutUser === undefined ||
      (!withoutUser && users === undefined) ||
      issueRange === undefined ||
      issueRange?.start === undefined ||
      issueRange?.end === undefined
    ) {
      return;
    }

    const isValuePresentInFilters =
      withoutStatus ||
      withoutType ||
      withoutUser ||
      !!status ||
      !!type ||
      !!users ||
      !!issueRange.start ||
      !!issueRange.end;

    this.filteredTopics = isValuePresentInFilters
      ? [
          ...this.issueFilterService.filterIssue(
            this.bcfFile.topics,
            withoutStatus,
            status || '',
            withoutType,
            type || '',
            withoutUser,
            users || [],
            issueRange?.start,
            issueRange?.end
          ),
        ]
      : this.bcfFile.topics;
  }

  addNavisworksClashIssues(): void {
    this.dialog
      .open(NavisworksClashSelectionComponent)
      .afterClosed()
      .subscribe(
        (selection?: {
          clashId: string;
          onlyImportNew: boolean;
          statusType: string | null;
        }) => {
          if (!selection) {
            return;
          }
          this.notificationsService.info(
            'If there are many clashes, generation of the data could take a few minutes.'
          );
          this.navisworksClashesLoadingService.showLoadingScreen();

          const existingIds = selection.onlyImportNew
            ? this.bcfFile.topics
                .filter(
                  (topic) =>
                    !!topic.serverAssignedId &&
                    // We only want to take Guids, as other server assigned ids might not originate from Navisworks
                    /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(
                      topic.serverAssignedId
                    )
                )
                .map((topic) => topic.serverAssignedId!)
            : [];

          const cancellationSubscription =
            this.navisworksClashProgressMessengerService.cancelGeneration.subscribe(
              () => {
                this.viewpointsClient
                  .cancelNavisworksClashDetection(selection.clashId)
                  .subscribe(() => {
                    /* Not doing anything with the response, that's handled later */
                  });
              }
            );

          this.viewpointsClient
            .createNavisworksClashDetectionResultIssues({
              clashId: selection.clashId,
              excludedClashIds: existingIds,
              status: selection.statusType,
            })
            .subscribe({
              next: (createdTopics) => {
                this.navisworksClashesLoadingService.hideLoadingScreen();
                cancellationSubscription.unsubscribe();
                this.settingsMessengerService.settings
                  .pipe(take(1))
                  .subscribe((s) => {
                    createdTopics.forEach((topic) => {
                      topic.creationAuthor = s.username;
                    });

                    if (selection.onlyImportNew) {
                      // In that case, we're filtering out those topics that already exist in the
                      createdTopics = createdTopics.filter(
                        (topic) =>
                          !this.bcfFile.topics.some(
                            (existingTopic) =>
                              existingTopic.serverAssignedId ===
                              topic.serverAssignedId
                          )
                      );
                    }

                    this.bcfFile.topics.push(...createdTopics);
                    this.filteredTopics = [...this.bcfFile.topics];
                    this.bcfFileAutomaticallySaveService.saveCurrentActiveBcfFileAutomatically();

                    this.teamsMessengerService.sendMessageToTeams(
                      MessageType.AddNavisworksClashes
                    );
                  });
              },
              error: (error) => {
                this.navisworksClashesLoadingService.hideLoadingScreen();
                cancellationSubscription.unsubscribe();
                console.error(error);
                this.notificationsService.error(
                  'Failed to generate the clash data from Navisworks, this is probably a timeout issue'
                );
              },
            });
        }
      );
  }

  setResponsibleForAll(): void {
    this.dialog
      .open(BulkTopicEditComponent)
      .afterClosed()
      .subscribe(
        (bulkOptions?: {
          responsibleUser?: string;
          status?: string;
          type?: string;
        }) => {
          if (!bulkOptions) {
            return;
          }

          this.filteredTopics.forEach((topic) => {
            if (bulkOptions.status) {
              topic.topicStatus =
                bulkOptions.status === '' ? undefined : bulkOptions.status;
            }
            if (bulkOptions.type) {
              topic.topicType =
                bulkOptions.type === '' ? undefined : bulkOptions.type;
            }
            if (bulkOptions.responsibleUser) {
              topic.assignedTo =
                bulkOptions.responsibleUser === ''
                  ? undefined
                  : bulkOptions.responsibleUser;
            }
          });

          this.bcfFileAutomaticallySaveService.saveCurrentActiveBcfFileAutomatically();
        }
      );
  }
}
