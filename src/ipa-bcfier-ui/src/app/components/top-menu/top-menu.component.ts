import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { BackendService } from '../../services/BackendService';
import { BcfFileWrapper } from '../../generated-client/generated-client';
import { BcfFilesMessengerService } from '../../services/bcf-files-messenger.service';
import { Component, OnDestroy, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { NotificationsService } from '../../services/notifications.service';
import { SettingsComponent } from '../settings/settings.component';
import { version } from '../../version';
import { AsyncPipe, UpperCasePipe } from '@angular/common';
import { SelectedProjectMessengerService } from '../../services/selected-project-messenger.service';
import { MatMenuModule } from '@angular/material/menu';
import { LastOpenedFilesComponent } from '../last-opened-files/last-opened-files.component';
import {
  Subject,
  combineLatestWith,
  delay,
  of,
  switchMap,
  take,
  takeUntil,
} from 'rxjs';
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
  ],
  templateUrl: './top-menu.component.html',
  styleUrl: './top-menu.component.scss',
})
export class TopMenuComponent implements OnDestroy {
  private destroyed$ = new Subject<void>();
  version = version.version;
  selectedProject$ = inject(SelectedProjectMessengerService).selectedProject;
  lastFileMenuOpened = false;
  lastOpenedFiles: any[] = [];
  constructor(
    private backendService: BackendService,
    private notificationsService: NotificationsService,
    private bcfFilesMessengerService: BcfFilesMessengerService,
    private matDialog: MatDialog
  ) {
    this.checkOpenedFileAndSendInfo();
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

  checkOpenedFileAndSendInfo(): void {
    this.bcfFilesMessengerService.bcfFileSelected
      .pipe(
        takeUntil(this.destroyed$),
        combineLatestWith(this.selectedProject$)
      )
      .subscribe(([f, p]) => {
        //TODO add new backend method to send data
        console.log(f.fileName);
        console.log(p?.id || null);
      });
  }

  showLastOpenedFiles(): void {
    this.lastFileMenuOpened = true;
    this.selectedProject$
      .pipe(
        take(1),
        switchMap((p) => {
          console.log(p?.id || null);
          //TODO add new backend method to get data
          return of([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]).pipe(delay(1000));
        })
      )
      .subscribe((f) => {
        this.lastOpenedFiles = [...f];
      });
  }
}
