import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { BackendService } from '../../services/BackendService';
import { BcfFileWrapper } from '../../generated-client/generated-client';
import { BcfFilesMessengerService } from '../../services/bcf-files-messenger.service';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { NotificationsService } from '../../services/notifications.service';
import { SettingsComponent } from '../settings/settings.component';
import { version } from '../../version';
import { AsyncPipe, UpperCasePipe } from '@angular/common';
import { SelectedProjectMessengerService } from '../../services/selected-project-messenger.service';

@Component({
  selector: 'bcfier-top-menu',
  standalone: true,
  imports: [
    MatIconModule,
    MatButtonModule,
    MatDialogModule,
    UpperCasePipe,
    AsyncPipe,
  ],
  templateUrl: './top-menu.component.html',
  styleUrl: './top-menu.component.scss',
})
export class TopMenuComponent {
  version = version.version;
  selectedProject$ = inject(SelectedProjectMessengerService).selectedProject;
  constructor(
    private backendService: BackendService,
    private notificationsService: NotificationsService,
    private bcfFilesMessengerService: BcfFilesMessengerService,
    private matDialog: MatDialog
  ) {}

  openBcf(): void {
    this.backendService.importBcfFile().subscribe({
      next: (bcfFileWrapper: BcfFileWrapper) => {
        this.bcfFilesMessengerService.openBcfFile(bcfFileWrapper);
      },
      error: (error) => {
        this.notificationsService.error('Error during BCF import.');
      },
    });
  }

  newBcfFile(): void {
    this.bcfFilesMessengerService.createNewBcfFile();
  }

  openSettings(): void {
    this.matDialog.open(SettingsComponent, {
      autoFocus: false,
      width: '80%',
      maxHeight: '70vh',
      restoreFocus: false,
    });
  }

  openDocumentation(): void {
    this.backendService.openDocumentation();
  }

  saveBcfAs(): void {
    this.bcfFilesMessengerService.saveCurrentActiveBcfFileAs();
  }
}
