import { Injectable, inject } from '@angular/core';
import { combineLatestWith, take } from 'rxjs';
import { TopicMessengerService } from './topic-messenger.service';
import { SettingsMessengerService } from './settings-messenger.service';

export enum MessageType {
  AddComment = 'AddNomment',
  AddViewpoint = 'AddViewpoint',
  ChangeTitle = 'ChangeTitle',
}
@Injectable({
  providedIn: 'root',
})
export class TeamsMessengerService {
  private topicMessengerService = inject(TopicMessengerService);
  private settingsMessengerService = inject(SettingsMessengerService);
  constructor() {}

  sendMessageToTeams(messageType: MessageType): void {
    switch (messageType) {
      case MessageType.AddComment:
        this.sendInfoAboutAddComment();
        break;
      case MessageType.AddViewpoint:
        this.sendInfoAboutAddViewpoint();
        break;
      case MessageType.ChangeTitle:
        this.sendInfoAboutChangeTitle();
        break;
      default:
        return;
    }
  }

  private sendInfoAboutAddComment(): void {
    this.topicMessengerService.selectedTopic.pipe(take(1)).subscribe((t) => {
      const lastAddedComment = t?.comments[t?.comments.length - 1];
      console.log(lastAddedComment?.author);
      console.log(lastAddedComment?.text);
      //TODO Send data to new backend method
    });
  }

  private sendInfoAboutAddViewpoint(): void {
    this.topicMessengerService.selectedTopic
      .pipe(take(1), combineLatestWith(this.settingsMessengerService.settings))
      .subscribe(([t, s]) => {
        const lastAddedViewpoint = t?.viewpoints[t?.viewpoints.length - 1];
        console.log(lastAddedViewpoint?.snapshotBase64);
        console.log(s.username);
        //TODO Send data to new backend method
      });
  }

  private sendInfoAboutChangeTitle(): void {
    this.topicMessengerService.selectedTopic.pipe(take(1)).subscribe((t) => {
      console.log(t?.title);
      console.log(t?.creationAuthor);
      //TODO Send data to new backend method
    });
  }
}
