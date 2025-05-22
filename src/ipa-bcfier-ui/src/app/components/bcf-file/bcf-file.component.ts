import {
  BcfFile,
  BcfTopic,
  ProjectGet,
  ProjectsClient,
  NavisworksClashGroupingData,
  ViewpointsClient,
} from '../../generated-client/generated-client';
import {
  ChangeDetectorRef,
  Component,
  Input,
  TemplateRef,
  ViewChild,
  inject,
} from '@angular/core';
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
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';
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
import { ProjectsService } from '../../services/light-query/projects.service';
import { ReviteProjectMessengerService } from '../../services/messengers/revite-project-messenger.service';
import { SafeUrlPipe } from '../../pipes/safe-url.pipe';
import { SelectedProjectMessengerService } from '../../services/selected-project-messenger.service';
import { SettingsMessengerService } from '../../services/settings-messenger.service';
import { TopicDetailComponent } from '../topic-detail/topic-detail.component';
import { TopicFilterPipe } from '../../pipes/topic-filter.pipe';
import { TopicMessengerService } from '../../services/topic-messenger.service';
import { TopicPreviewImageDirective } from '../../directives/topic-preview-image.directive';
import { TriangleCornerDirective } from '../../directives/triangle-corner.directive';
import { getNewRandomGuid } from '../../functions/uuid';
import { of, switchMap, take } from 'rxjs';
import { ClashGroupingOptionsComponent } from '../clash-grouping-options/clash-grouping-options.component';

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
    TriangleCornerDirective,
  ],
  providers: [TopicFilterPipe],
  templateUrl: './bcf-file.component.html',
  styleUrl: './bcf-file.component.scss',
})
export class BcfFileComponent {
  @Input() bcfFile!: BcfFile;

  @ViewChild('revitDialogContent', { static: true })
  revitDialogContent!: TemplateRef<unknown>;

  projectsService = inject(ProjectsService);
  issueStatuses$ = inject(IssueStatusesService).issueStatuses;
  issueTypes$ = inject(IssueTypesService).issueTypes;
  users$ = inject(ProjectUsersService).users;
  issueFilterService = inject(IssueFilterService);
  filterPipe = inject(TopicFilterPipe).transform;
  bcfFileAutomaticallySaveService = inject(BcfFileAutomaticallySaveService);
  teamsMessengerService = inject(TeamsMessengerService);
  topicMessengerService = inject(TopicMessengerService);
  settingsMessengerService = inject(SettingsMessengerService);
  appConfigService = inject(AppConfigService);
  navisworksClashProgressMessengerService = inject(
    NavisworksClashProgressMessengerService
  );
  cdr = inject(ChangeDetectorRef);
  selectedTopic: BcfTopic | null = null;
  selectedListTopic: BcfTopic[] = [];
  filteredTopics: BcfTopic[] = [];
  isInNavisworks =
    inject(AppConfigService).getFrontendConfig().isConnectedToNavisworks;
  viewpointsClient = inject(ViewpointsClient);
  navisworksClashesLoadingService = inject(NavisworksClashesLoadingService);
  private reviteProjectMessengerService = inject(ReviteProjectMessengerService);
  notificationsService = inject(NotificationsService);
  private dialog = inject(MatDialog);
  readonly STATUS_COLOR_MAP: Record<string, string> = {
    new: '#ff0000', // Red
    open: '#ff0000', // Red
    reopened: '#ff0000', // Red
    active: '#ffa500', // Orange
    reviewed: '#00cfff', // Cyan
    approved: '#00ff00', // Green
    resolved: '#ffff00', // Yellow
  };
  private projectsClient = inject(ProjectsClient);
  private selectedProjectMessengerService = inject(
    SelectedProjectMessengerService
  );
  selectedProject: ProjectGet | null = null;

  ngOnInit() {
    if (!this.bcfFile) return;
    this.oneSelectTopic(this.bcfFile.topics[0] || null);
    this.cdr.detectChanges();
    this.filteredTopics = [...this.bcfFile.topics];

    //Here we get messages only if app is connected to Revit
    this.reviteProjectMessengerService.revitProject.subscribe((project) => {
      if (project) {
        this.findRevitProjectInDatabase(
          project.projectNumber,
          project.filePath
        );
      }
    });
  }

  private _search = '';
  public set search(value: string) {
    this._search = value;
  }
  public get search(): string {
    return this._search;
  }

  selectTopic(topic: BcfTopic, event: MouseEvent): void {
    event.stopPropagation();
    event.preventDefault();
    if (event.ctrlKey) {
      this.addTopicToSelectedList(topic);
    } else if (event.shiftKey) {
      this.addRangeToSelectedList(topic);
    } else {
      this.oneSelectTopic(topic);
    }
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
        assignedToList: [],
        creationAuthor: s.username,
        description: '',
        priority: '',
        title: 'New Issue',
        topicStatus: '',
        stage: '',
        topicTypes: [],
        serverAssignedId: '',
        modifiedAuthor: '',
        creationDate: new Date(),
      };

      if (this.appConfigService.getFrontendConfig().isConnectedToRevit) {
        // We'll add a server assigned id like "Revit_<GUID>"
        newIssue.serverAssignedId = `Revit_${getNewRandomGuid()}`;
      }

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
          clashIds: string[];
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
                  .cancelNavisworksClashDetection(selection.clashIds)
                  .subscribe(() => {
                    /* Not doing anything with the response, that's handled later */
                  });
              }
            );

          this.viewpointsClient
            .createNavisworksClashDetectionResultIssues({
              clashIds: selection.clashIds,
              excludedClashIds: existingIds,
              status: selection.statusType,
              shouldMoveBoundingBoxToCenterOfClash: true,
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

                    // Now we're trying to change topics that we have already imported if their status
                    // has changed, so we don't import them again but just update their status

                    const topicsToAdd: BcfTopic[] = [];
                    createdTopics.forEach((createdTopic) => {
                      // We'll check if it exists already, and if it does, we'll just update the status
                      var existingTopic = this.bcfFile.topics.find(
                        (existing) =>
                          existing.serverAssignedId ===
                          createdTopic.serverAssignedId
                      );
                      if (existingTopic) {
                        existingTopic.topicStatus = createdTopic.topicStatus;
                      } else {
                        topicsToAdd.push(createdTopic);
                      }
                    });

                    this.bcfFile.topics.push(...topicsToAdd);
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

  setResponsibleForAll(selectingMode?: boolean): void {
    // Avoid warning in console (problem in Angular v.19)
    (document.activeElement as HTMLElement)?.blur();

    this.dialog
      .open(BulkTopicEditComponent, {
        data: { selectingMode },
        autoFocus: false,
        restoreFocus: false,
      })
      .afterClosed()
      .subscribe(
        (bulkOptions?: {
          responsibleUser: string[];
          status?: string;
          type: string[];
          dueDate?: Date;
          additionalMode: boolean;
        }) => {
          if (!bulkOptions) {
            return;
          }

          const list = selectingMode
            ? this.selectedListTopic
            : this.filterPipe(this.filteredTopics, this.search);

          list.forEach((topic) => {
            if (bulkOptions.status) {
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
                    topic.assignedToList = [
                      ...(topic.assignedToList || []),
                      user,
                    ];
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
        }
      );
  }

  private findRevitProjectInDatabase(
    projectNumber: string,
    filePath: string
  ): void {
    this.projectsService.getAll().subscribe((projects) => {
      if (projects?.length && projects.length > 0) {
        let selectedProject =
          projects.find(
            (p) =>
              p.number === projectNumber &&
              p.revitFilePath === filePath &&
              p.number?.length > 0
          ) ||
          projects.find((p) => p.revitFilePath === filePath) ||
          projects.find(
            (p) => p.number === projectNumber && p.number?.length > 0
          );

        if (
          this.selectedProjectMessengerService.lastSelectedProjectId ===
            selectedProject?.id ||
          !selectedProject
        ) {
          // In that case, we don't want to show the dialog and just keep everything as-is
          return;
        }

        this.selectedProject = selectedProject;
        this.dialog
          .open(ConfirmDialogComponent, {
            autoFocus: false,
            restoreFocus: false,
            data: { contentTemplate: this.revitDialogContent },
          })
          .afterClosed()
          .subscribe((confirm) => {
            if (confirm) {
              this.selectedProjectMessengerService.setSelectedProject(
                this.selectedProject
              );
            } else {
              this.selectedProject = null;
              this.reviteProjectMessengerService.setRevitProject(
                this.selectedProject
              );
            }
          });
      } else {
        this.selectedProject = null;
        this.reviteProjectMessengerService.setRevitProject(
          this.selectedProject
        );
      }
    });
  }

  oneSelectTopic(topic: BcfTopic | null): void {
    this.selectedTopic = topic;
    if (topic) {
      this.selectedListTopic = [topic];
    } else {
      this.selectedListTopic = [];
    }
    this.topicMessengerService.setSelectedTopic(this.selectedTopic);
  }

  addTopicToSelectedList(topic: BcfTopic): void {
    this.selectedTopic = topic;
    if (!this.inSelectedList(topic.id)) {
      this.selectedListTopic.push(topic);
    }
    this.topicMessengerService.setSelectedTopic(this.selectedTopic);
  }

  addRangeToSelectedList(topic: BcfTopic): void {
    if (this.selectedTopic) {
      const indexFirst = this.filteredTopics.findIndex(
        (item) => item.id === this.selectedTopic?.id
      );
      const indexLast = this.filteredTopics.findIndex(
        (item) => item.id === topic.id
      );
      const direction = indexFirst < indexLast ? 1 : -1;
      for (let i = indexFirst; i !== indexLast + direction; i += direction) {
        const topic = this.filteredTopics[i];
        if (!this.inSelectedList(topic.id)) {
          this.selectedListTopic.push(topic);
        }
      }
      this.selectedTopic = topic;
      this.topicMessengerService.setSelectedTopic(this.selectedTopic);
    }
  }

  inSelectedList(id: string): boolean {
    return !!this.selectedListTopic.find((item) => item.id === id);
  }

  openGroupingDialog(): void {
    this.dialog
      .open(ClashGroupingOptionsComponent, {
        autoFocus: false,
        restoreFocus: false,
      })
      .afterClosed()
      .pipe(
        switchMap((groupingOptions: NavisworksClashGroupingData) => {
          if (!groupingOptions) {
            return of([]);
          }
          return this.viewpointsClient.groupClashes(groupingOptions);
        })
      )
      .subscribe({
        next: (clashes) => {
          //TODO - add the clashes to the BCF file, but now it returns string array
        },
        error: (error) => {
          console.error(error);
          this.notificationsService.error('Failed to group the clashes');
        },
      });
  }
}
