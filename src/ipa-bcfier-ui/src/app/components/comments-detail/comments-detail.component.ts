import {
  BcfComment,
  BcfTopic,
  BcfViewpoint,
} from '../../generated-client/generated-client';
import { Component, Input, OnInit } from '@angular/core';
import {
  MessageType,
  TeamsMessengerService,
} from '../../services/teams-messenger.service';

import { BackendService } from '../../services/BackendService';
import { BcfFileAutomaticallySaveService } from '../../services/bcf-file-automaticaly-save.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ImagePreviewComponent } from '../image-preview/image-preview.component';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { NotificationsService } from '../../services/notifications.service';
import { SettingsMessengerService } from '../../services/settings-messenger.service';
import { ViewpointImageDirective } from '../../directives/viewpoint-image.directive';
import { getNewRandomGuid } from '../../functions/uuid';
import { of, take, throwError } from 'rxjs';
import {
  ElementsViewpointComponent,
  elementClash,
} from '../elements-viewpoint/elements-viewpoint.component';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'bcfier-comments-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    ViewpointImageDirective,
    ElementsViewpointComponent,
    MatTooltipModule,
  ],
  templateUrl: './comments-detail.component.html',
  styleUrl: './comments-detail.component.scss',
})
export class CommentsDetailComponent implements OnInit {
  @Input() comments!: BcfComment[];
  @Input() viewpoint: BcfViewpoint | null = null;
  @Input() topic!: BcfTopic;
  viewpointElements: elementClash[] = [];

  newComment = '';

  constructor(
    private settingsMessengerService: SettingsMessengerService,
    private notificationsService: NotificationsService,
    private matDialog: MatDialog,
    private backendService: BackendService,
    private bcfFileAutomaticallySaveService: BcfFileAutomaticallySaveService,
    private teamsMessengerService: TeamsMessengerService
  ) {}

  ngOnInit(): void {
    if (this.viewpoint) {
      this.getListElement();
    }
  }

  addComment(): void {
    if (!this.newComment) {
      return;
    }

    this.settingsMessengerService.settings
      .pipe(take(1))
      .subscribe((settings) => {
        const newComment = {
          id: getNewRandomGuid(),
          author: settings.username,
          creationDate: new Date(),
          viewpointId: this.viewpoint?.id,
          text: this.newComment,
        };

        this.topic.comments.push(newComment);
        this.newComment = '';

        this.notificationsService.success('Comment added');
        this.bcfFileAutomaticallySaveService.saveCurrentActiveBcfFileAutomatically();
        this.teamsMessengerService.sendMessageToTeams(MessageType.AddComment);
      });
  }

  removeComment(comment: BcfComment): void {
    this.topic.comments = this.topic.comments.filter(
      (c) => c.id !== comment.id
    );

    this.bcfFileAutomaticallySaveService.saveCurrentActiveBcfFileAutomatically();
    this.notificationsService.success('Comment removed');
  }

  deleteViewpoint(viewpoint: BcfViewpoint): void {
    this.topic.viewpoints = this.topic.viewpoints.filter(
      (v) => v.id !== viewpoint.id
    );

    this.topic.comments.forEach((c) => {
      if (c.viewpointId === viewpoint.id) {
        c.viewpointId = undefined;
      }
    });

    // When we're deleting a viewpoint, we also want to ensure that
    // all other places where the comments are used are reevaluated,
    // since comments that originally belonged to the viewpoint
    // are moved to general comments now
    this.topic.comments = [...this.topic.comments];
    this.bcfFileAutomaticallySaveService.saveCurrentActiveBcfFileAutomatically();
  }

  showImageFullScreen(viewpoint: BcfViewpoint): void {
    this.matDialog.open(ImagePreviewComponent, {
      data: viewpoint,
    });
  }

  selectViewpoint(): void {
    if (this.viewpoint) {
      const viewpointOriginatesFromRevit =
        !!this.topic &&
        !!this.topic.serverAssignedId &&
        /^Revit_/i.test(this.topic.serverAssignedId);
      this.backendService.selectViewpoint(
        viewpointOriginatesFromRevit,
        this.viewpoint
      );
    }
  }

  trySelectElement(element: elementClash): void {
    // TODO: remove this mock request and use the real request from the backend
    of(element)
    // throwError(() => new Error()) // error case
      .subscribe({
        next: () => {
          this.notificationsService.success('Element selected: ' + element.name);
        },
        error: () => {
          this.notificationsService.error('Error selecting element: ' + element.name);
        },
      });
  }

  getListElement(): void {
    //TODO: remove this mock data and use the real data from the backend
    of([
      { name: 'name1', id: '1' },
      { name: 'name2', id: '2' },
      { name: 'name3', id: '3' },
      { name: 'name4', id: '4' },
      { name: 'name5', id: '5' },
      { name: 'name6', id: '6' },
      { name: 'name7', id: '7' },
      { name: 'name8', id: '8' },
      { name: 'name9', id: '9' },
      { name: 'name10', id: '10' },
      { name: 'name11', id: '11' },
      { name: 'name12', id: '12' },
      { name: 'name13', id: '13' },
      { name: 'name14', id: '14' },
      { name: 'name15', id: '15' },
    ])
    // throwError(() => new Error()) // error case
    .subscribe({
      next: (list) => {
        this.viewpointElements = list;
      },
      error: () => {
        this.notificationsService.error('Error fetching list of elements');
      },
    });
  }
}
