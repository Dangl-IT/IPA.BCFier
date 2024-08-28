import { AsyncPipe, UpperCasePipe } from '@angular/common';
import {
  BcfFileWrapper,
  LastOpenedFileGet,
  LastOpenedFilesClient,
  SettingsClient,
} from '../../generated-client/generated-client';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import {
  Subject,
  combineLatestWith,
  delay,
  distinctUntilChanged,
  of,
  switchMap,
  take,
  takeUntil,
} from 'rxjs';

import { BackendService } from '../../services/BackendService';
import { BcfFilesMessengerService } from '../../services/bcf-files-messenger.service';
import { FormsModule } from '@angular/forms';
import { LastOpenedFilesComponent } from '../last-opened-files/last-opened-files.component';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { NotificationsService } from '../../services/notifications.service';
import { SelectedProjectMessengerService } from '../../services/selected-project-messenger.service';
import { SettingsComponent } from '../settings/settings.component';
import { version } from '../../version';

@Component({
  selector: 'bcfier-top-menu',
  standalone: true,
  imports: [
    MatIconModule,
    MatButtonModule,
    MatDialogModule,
    UpperCasePipe,
    AsyncPipe,
    MatMenuModule,
    LastOpenedFilesComponent,
    MatCheckboxModule,
    FormsModule,
  ],
  templateUrl: './top-menu.component.html',
  styleUrl: './top-menu.component.scss',
})
export class TopMenuComponent implements OnDestroy, OnInit {
  private destroyed$ = new Subject<void>();
  version = version.version;
  selectedProject$ = inject(SelectedProjectMessengerService).selectedProject;
  lastFileMenuOpened = false;
  lastOpenedFiles: LastOpenedFileGet[] = [];
  alwaysOnTopRequestRunning = false;
  private _alwaysOnTop = true;
  set alwaysOnTop(value: boolean) {
    this._alwaysOnTop = value;
    this.changeAlwaysOnTopInBackend();
  }
  get alwaysOnTop(): boolean {
    return this._alwaysOnTop;
  }

  constructor(
    private backendService: BackendService,
    private notificationsService: NotificationsService,
    private bcfFilesMessengerService: BcfFilesMessengerService,
    private matDialog: MatDialog,
    private lastOpenedFilesClient: LastOpenedFilesClient,
    private settingsClient: SettingsClient
  ) {
    this.checkOpenedFileAndSendInfo();
  }

  private changeAlwaysOnTopInBackend(): void {
    const isAlwaysOnTop = this._alwaysOnTop;
    this.alwaysOnTopRequestRunning = true;

    this.settingsClient.getIsAlwaysOnTop().subscribe({
      next: (serverIsAlwaysOnTop) => {
        if (serverIsAlwaysOnTop === isAlwaysOnTop) {
          this.alwaysOnTopRequestRunning = false;
          return;
        }

        this.settingsClient.setIsAlwaysOnTop(isAlwaysOnTop).subscribe({
          next: () => {
            this.alwaysOnTopRequestRunning = false;
          },
          error: () => {
            this.notificationsService.error(
              'Could not change always on top setting.'
            );
            this.alwaysOnTopRequestRunning = false;
          },
        });
      },
      error: () => {
        this.notificationsService.error(
          'Could not change always on top setting.'
        );
        this.alwaysOnTopRequestRunning = false;
      },
    });
  }

  ngOnInit(): void {
    this.settingsClient.getIsAlwaysOnTop().subscribe((isAlwaysOnTop) => {
      this._alwaysOnTop = isAlwaysOnTop;
    });
  }

  ngOnDestroy(): void {
    this.destroyed$.next();
    this.destroyed$.complete();
  }

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

  private checkOpenedFileAndSendInfo(): void {
    this.bcfFilesMessengerService.bcfFileSelected
      .pipe(distinctUntilChanged(), takeUntil(this.destroyed$))
      .subscribe((bcfFile) => {
        if (bcfFile.fileName) {
          this.selectedProject$.pipe(take(1)).subscribe((project) => {
            this.lastOpenedFilesClient
              .setFileAsLastOpened(project?.id, bcfFile.fileName)
              .subscribe(() => {
                // Not doing anything with the response
              });
          });
        }
      });
  }

  showLastOpenedFiles(): void {
    this.lastFileMenuOpened = true;
    this.selectedProject$
      .pipe(
        take(1),
        switchMap((p) => {
          return this.lastOpenedFilesClient.getLastOpenedFiles(p?.id);
        })
      )
      .subscribe((f) => {
        if (f.lastOpenedFiles.length === 0) {
          this.notificationsService.info('No last opened files found.');
        }
        this.lastOpenedFiles = [...f.lastOpenedFiles];
      });
  }
}
