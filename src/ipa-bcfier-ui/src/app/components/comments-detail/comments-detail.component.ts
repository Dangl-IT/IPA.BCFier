import {
  BcfComment,
  BcfTopic,
  BcfViewpoint,
  IfcGuidNamePair,
} from '../../generated-client/generated-client';
import { Component, inject, Input, OnInit } from '@angular/core';
import {
  MessageType,
  TeamsMessengerService,
} from '../../services/teams-messenger.service';

import { BackendService } from '../../services/BackendService';
import { BcfFileAutomaticallySaveService } from '../../services/bcf-file-automaticaly-save.service';
import { CommonModule } from '@angular/common';
import { ElementsViewpointComponent } from '../elements-viewpoint/elements-viewpoint.component';
import { FormsModule } from '@angular/forms';
import { ImagePreviewComponent } from '../image-preview/image-preview.component';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NotificationsService } from '../../services/notifications.service';
import { SettingsMessengerService } from '../../services/settings-messenger.service';
import { ViewpointImageDirective } from '../../directives/viewpoint-image.directive';
import { getNewRandomGuid } from '../../functions/uuid';
import { take } from 'rxjs';
import { AppConfigService } from '../../services/AppConfigService';

@Component({
  selector: 'bcfier-comments-detail',
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    ViewpointImageDirective,
    MatTooltipModule,
    ElementsViewpointComponent,
  ],
  templateUrl: './comments-detail.component.html',
  styleUrl: './comments-detail.component.scss',
})
export class CommentsDetailComponent implements OnInit {
  @Input() comments!: BcfComment[];
  @Input() viewpoint: BcfViewpoint | null = null;
  @Input() topic!: BcfTopic;
  viewpointElements: any[] = [];
  private appConfigService = inject(AppConfigService);
  isConnectedToRevit =
    this.appConfigService.getFrontendConfig().isConnectedToRevit || true;
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
    if (this.viewpoint && this.isConnectedToRevit) {
      // TODO rewrite this function after backend will update
      // this.getListElement(this.viewpoint);
      this.viewpointElements = [
        {
          name: 'Model A',
          components: [
            {
              originatingSystem: 'Wall',
              authoringToolId: '456',
              ifcGuid: 'abc-001',
            },
            {
              originatingSystem: 'Column',
              authoringToolId: '123',
              ifcGuid: 'abc-002',
            },
            {
              originatingSystem: 'Door',
              authoringToolId: '789',
              ifcGuid: 'abc-003',
            },
            {
              originatingSystem: 'Window',
              authoringToolId: '321',
              ifcGuid: 'abc-004',
            },
            {
              originatingSystem: 'Roof',
              authoringToolId: '654',
              ifcGuid: 'abc-005',
            },
          ],
        },
        {
          name: 'Model B',
          components: [
            {
              originatingSystem: 'Wall',
              authoringToolId: '123',
              ifcGuid: 'def-001',
            },
            {
              originatingSystem: 'Window',
              authoringToolId: '789',
              ifcGuid: 'def-002',
            },
            {
              originatingSystem: 'Slab',
              authoringToolId: '147',
              ifcGuid: 'def-003',
            },
            {
              originatingSystem: 'Beam',
              authoringToolId: '258',
              ifcGuid: 'def-004',
            },
            {
              originatingSystem: 'Column',
              authoringToolId: '369',
              ifcGuid: 'def-005',
            },
          ],
        },
      ];
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

  trySelectElement(element: IfcGuidNamePair): void {
    this.backendService.selectElement(element).subscribe({
      next: () => {
        this.notificationsService.success('Element selected: ' + element.name);
      },
      error: () => {
        this.notificationsService.error(
          'Error selecting element: ' + element.name
        );
      },
    });
  }

  getListElement(viewpoint: BcfViewpoint): void {
    const selectedComponentIfcGuids =
      viewpoint?.viewpointComponents?.selectedComponents?.map((component) => {
        return {
          ifcGuid: component.ifcGuid,
          revitId: component.authoringToolId,
          name: '',
        } as IfcGuidNamePair;
      }) || [];

    if (selectedComponentIfcGuids.length === 0) {
      this.viewpointElements = [];
      return;
    }

    this.backendService
      .getElementNamesList(selectedComponentIfcGuids)
      .subscribe({
        next: (list: IfcGuidNamePair[]) => {
          this.viewpointElements = list;
        },
        error: () => {
          this.notificationsService.error('Error fetching list of elements');
        },
      });
  }
}
