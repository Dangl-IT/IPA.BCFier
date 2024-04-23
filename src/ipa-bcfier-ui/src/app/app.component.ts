import { Component, OnDestroy, ViewChild } from '@angular/core';
import { MatTabGroup, MatTabsModule } from '@angular/material/tabs';
import {
  Observable,
  Subject,
  combineLatest,
  filter,
  map,
  of,
  switchMap,
  take,
  takeUntil,
} from 'rxjs';

import { BackendService } from './services/BackendService';
import { BcfFile, BcfFileWrapper } from '../generated/models';
import { BcfFileAutomaticallySaveService } from './services/bcf-file-automaticaly-save.service';
import { BcfFileComponent } from './components/bcf-file/bcf-file.component';
import { BcfFilesMessengerService } from './services/bcf-files-messenger.service';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { NotificationsService } from './services/notifications.service';
import { TopMenuComponent } from './components/top-menu/top-menu.component';

@Component({
  selector: 'bcfier-root',
  standalone: true,
  imports: [
    MatToolbarModule,
    MatTabsModule,
    TopMenuComponent,
    CommonModule,
    BcfFileComponent,
    MatIconModule,
    MatButtonModule,
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent implements OnDestroy {
  bcfFiles: Observable<BcfFileWrapper[]>;
  @ViewChild(MatTabGroup) tabGroup: MatTabGroup | undefined;
  private destroyed$ = new Subject<void>();
  constructor(
    private bcfFilesMessengerService: BcfFilesMessengerService,
    private backendService: BackendService,
    private notificationsService: NotificationsService,
    private bcfFileAutomaticallySaveService: BcfFileAutomaticallySaveService
  ) {
    this.bcfFiles = bcfFilesMessengerService.bcfFiles;
    this.bcfFilesMessengerService.bcfFileSaveRequested
      .pipe(
        takeUntil(this.destroyed$),
        switchMap(() => this.bcfFiles.pipe(take(1))),
        filter(
          (bcfFiles) =>
            !!this.tabGroup &&
            this.tabGroup.selectedIndex != null &&
            bcfFiles.length > this.tabGroup.selectedIndex &&
            !!bcfFiles.length
        ),
        map((bcfFiles) => {
          const selectedIndex = this.tabGroup?.selectedIndex as number;
          const selectedBcfFile = bcfFiles[selectedIndex];
          return selectedBcfFile;
        }),
        filter((selectedBcfFile) => !!selectedBcfFile),
        switchMap((selectedBcfFile) => {
          return this.backendService.exportBcfFile(selectedBcfFile);
        })
      )
      .subscribe({
        next: () => {
          this.notificationsService.success('BCF file saved successfully.');
        },
        error: (error) => {
          console.error('Error exporting BCF file:', error);
          this.notificationsService.error('Failed to save BCF file.');
        },
      });

    this.bcfFileAutomaticallySaveService.bcfFileSaveAutomaticallyRequested
      .pipe(
        takeUntil(this.destroyed$),
        switchMap(() => this.bcfFiles.pipe(take(1))),
        filter(
          (bcfFiles) =>
            !!this.tabGroup &&
            this.tabGroup.selectedIndex != null &&
            bcfFiles.length > this.tabGroup.selectedIndex &&
            !!bcfFiles.length
        ),
        map((bcfFiles) => {
          const selectedIndex = this.tabGroup?.selectedIndex as number;
          const selectedBcfFile = bcfFiles[selectedIndex];
          return selectedBcfFile;
        }),
        filter((selectedBcfFile) => !!selectedBcfFile),
        switchMap((selectedBcfFile) => {
          return this.backendService.saveBcfFile(selectedBcfFile);
        })
      )
      .subscribe({
        next: () => {
          this.notificationsService.success('BCF file saved successfully.');
        },
        error: (error) => {
          console.error('Error exporting BCF file:', error);
          this.notificationsService.error('Failed to save BCF file.');
        },
      });

    combineLatest([
      bcfFilesMessengerService.bcfFileSelected.pipe(takeUntil(this.destroyed$)),
      bcfFilesMessengerService.bcfFiles.pipe(take(1)),
    ]).subscribe(([bcfFile, bcfFiles]) => {
      this.updateTabIndex(
        bcfFile.bcfFile!,
        bcfFiles.map((f) => f.bcfFile!)
      );
    });
  }

  updateTabIndex(bcfFile: BcfFile, bcfFiles: BcfFile[]): void {
    if (this.tabGroup) {
      this.tabGroup.selectedIndex = bcfFiles.indexOf(bcfFile);
    }
  }

  ngOnDestroy(): void {
    this.destroyed$.next();
    this.destroyed$.complete();
  }

  closeBcfFile(bcfFile: BcfFile): void {
    this.bcfFilesMessengerService.closeBcfFile(bcfFile);
  }
}
