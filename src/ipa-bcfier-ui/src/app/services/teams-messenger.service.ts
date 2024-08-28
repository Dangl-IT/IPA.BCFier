import {
  EMPTY,
  Subject,
  combineLatestWith,
  switchMap,
  take,
  takeUntil,
} from 'rxjs';
import { Injectable, OnDestroy, inject } from '@angular/core';
import {
  TeamsMessagePost,
  TeamsMessagesClient,
} from '../generated-client/generated-client';

import { SelectedProjectMessengerService } from './selected-project-messenger.service';
import { SettingsMessengerService } from './settings-messenger.service';
import { TopicMessengerService } from './topic-messenger.service';

export enum MessageType {
  AddComment = 'AddNomment',
  AddViewpoint = 'AddViewpoint',
  ChangeTitle = 'ChangeTitle',
  AddNavisworksClashes = 'AddNavisworksClashes',
}
@Injectable({
  providedIn: 'root',
})
export class TeamsMessengerService implements OnDestroy {
  private topicMessengerService = inject(TopicMessengerService);
  private settingsMessengerService = inject(SettingsMessengerService);
  private selectedProjectMessengerService = inject(
    SelectedProjectMessengerService
  );
  private teamsMessagesClient = inject(TeamsMessagesClient);

  private destroyed$ = new Subject<void>();
  private projectId = '';
  constructor() {
    this.selectedProjectMessengerService.selectedProject
      .pipe(takeUntil(this.destroyed$))
      .subscribe((p) => {
        this.projectId = p?.id || '';
      });
  }
  ngOnDestroy(): void {
    this.destroyed$.next();
    this.destroyed$.complete();
  }

  sendMessageToTeams(messageType: MessageType): void {
    switch (messageType) {
      case MessageType.AddComment:
        this.sendInfoAboutAddComment();
        break;
      case MessageType.AddViewpoint:
        this.sendInfoAboutAddViewpoint();
        break;
      case MessageType.AddNavisworksClashes:
        this.sendInfoAboutAddNavisworksClashes();
        break;
      case MessageType.ChangeTitle:
        this.sendInfoAboutChangeTitle();
        break;
      default:
        return;
    }
  }

  private sendInfoAboutAddComment(): void {
    this.topicMessengerService.selectedTopic
      .pipe(
        take(1),
        switchMap((t) => {
          const lastAddedComment = t?.comments[t?.comments.length - 1];
          if (this.projectId && lastAddedComment?.author && t?.id) {
            const model: TeamsMessagePost = {
              comment: lastAddedComment?.text,
              username: lastAddedComment?.author,
            };
            return this.teamsMessagesClient.announceNewCommentInProjectTopic(
              this.projectId,
              t?.id,
              model
            );
          } else {
            return EMPTY;
          }
        })
      )
      .subscribe(() => {});
  }

  private sendInfoAboutAddViewpoint(): void {
    this.topicMessengerService.selectedTopic
      .pipe(
        take(1),
        combineLatestWith(this.settingsMessengerService.settings),
        switchMap(([t, s]) => {
          if (this.projectId && t?.id) {
            const lastAddedViewpoint = t?.viewpoints[t?.viewpoints.length - 1];
            const model: TeamsMessagePost = {
              viewpointBase64: lastAddedViewpoint.snapshotBase64,
              username: s.username,
            };
            return this.teamsMessagesClient.announceNewCommentInProjectTopic(
              this.projectId,
              t?.id,
              model
            );
          }
          return EMPTY;
        })
      )
      .subscribe(() => {});
  }

  private sendInfoAboutAddNavisworksClashes(): void {
    this.topicMessengerService.selectedTopic
      .pipe(
        take(1),
        combineLatestWith(this.settingsMessengerService.settings),
        switchMap(([t, s]) => {
          if (this.projectId && t?.id) {
            const lastAddedViewpoint = t?.viewpoints[t?.viewpoints.length - 1];
            const model: TeamsMessagePost = {
              comment: 'Clash Results from Navisworks added',
              username: s.username,
            };
            return this.teamsMessagesClient.announceNewCommentInProjectTopic(
              this.projectId,
              t?.id,
              model
            );
          }
          return EMPTY;
        })
      )
      .subscribe(() => {});
  }

  private sendInfoAboutChangeTitle(): void {
    this.topicMessengerService.selectedTopic
      .pipe(
        take(1),
        switchMap((t) => {
          if (this.projectId && t?.id && t.creationAuthor) {
            const model: TeamsMessagePost = {
              topicTitle: t.title,
              username: t.creationAuthor,
            };
            return this.teamsMessagesClient.announceNewCommentInProjectTopic(
              this.projectId,
              t?.id,
              model
            );
          }
          return EMPTY;
        })
      )

      .subscribe(() => {});
  }
}
