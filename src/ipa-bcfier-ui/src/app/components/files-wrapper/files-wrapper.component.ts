import { Component, inject, input, OnInit, viewChild } from '@angular/core';
import { BcfFileComponent } from '../bcf-file/bcf-file.component';
import { MatTabGroup, MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { BcfFilesMessengerService } from '../../services/bcf-files-messenger.service';
import {
  catchError,
  filter,
  map,
  Observable,
  of,
  Subject,
  switchMap,
  take,
  takeUntil,
  tap,
} from 'rxjs';
import {
  BcfFile,
  BcfFileWrapper,
} from '../../generated-client/generated-client';
import { NotificationsService } from '../../services/notifications.service';
import { BackendService } from '../../services/BackendService';
import { BcfFileAutomaticallySaveService } from '../../services/bcf-file-automaticaly-save.service';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'bcfier-files-wrapper',
  imports: [
    BcfFileComponent,
    MatTabsModule,
    MatIconModule,
    MatButtonModule,
    AsyncPipe,
  ],
  templateUrl: './files-wrapper.component.html',
  styleUrls: ['./files-wrapper.component.scss'],
})
export class FilesWrapperComponent implements OnInit {
  tabGroup = viewChild.required<MatTabGroup>(MatTabGroup);
  private destroyed$ = new Subject<void>();
  bcfFiles!: Observable<BcfFileWrapper[]>;

  private bcfFilesMessengerService = inject(BcfFilesMessengerService);
  private notificationsService = inject(NotificationsService);
  private backendService = inject(BackendService);
  private bcfFileAutomaticallySaveService = inject(
    BcfFileAutomaticallySaveService
  );

  ngOnDestroy(): void {
    this.destroyed$.next();
    this.destroyed$.complete();
  }

  ngOnInit(): void {
    this.bcfFiles = this.bcfFilesMessengerService.bcfFiles;
    this.changeSelectedTabIndex(0);

    if (this.tabGroup()) {
      this.bcfFilesMessengerService.bcfFileSaveAsRequested
        .pipe(
          takeUntil(this.destroyed$),
          switchMap(() => this.bcfFiles.pipe(take(1))),
          filter(
            (bcfFiles) =>
              !!this.tabGroup() &&
              this.tabGroup()?.selectedIndex != null &&
              !!bcfFiles.length
          ),
          map((bcfFiles) => {
            const selectedIndex = this.tabGroup()?.selectedIndex as number;
            const selectedBcfFile = bcfFiles[selectedIndex];
            return selectedBcfFile;
          }),
          filter((selectedBcfFile) => !!selectedBcfFile),
          switchMap((selectedBcfFile) => {
            return this.backendService.exportBcfFile(selectedBcfFile).pipe(
              tap((response) => {
                if (response && response.fileName) {
                  selectedBcfFile.fileName = response.fileName;
                  if (selectedBcfFile.bcfFile) {
                    selectedBcfFile.bcfFile.fileName =
                      response.fileName.replace(/^.*[\\/]/, '');
                  }
                }
              }),
              catchError((error) => {
                return of({ isError: true });
              })
            );
          })
        )
        .subscribe({
          next: (r) => {
            if ((r as any)?.isError) {
              console.error('Error during BCF file export.');
              this.notificationsService.error('Failed to save BCF file.');
            } else {
              this.notificationsService.success('BCF file saved successfully.');
            }
          },
          error: (error) => {
            console.error('Error while: exporting BCF file:', error);
            this.notificationsService.error('Failed to save BCF file.');
          },
        });

      this.bcfFileAutomaticallySaveService.bcfFileSaveAutomaticallyRequested
        .pipe(
          takeUntil(this.destroyed$),
          switchMap(() => this.bcfFiles.pipe(take(1))),
          filter(
            (bcfFiles) =>
              !!this.tabGroup() &&
              this.tabGroup()?.selectedIndex != null &&
              !!bcfFiles.length
          ),
          map((bcfFiles) => {
            const selectedIndex = this.tabGroup()?.selectedIndex as number;
            const selectedBcfFile = bcfFiles[selectedIndex];
            return selectedBcfFile;
          }),
          filter((selectedBcfFile) => !!selectedBcfFile),
          switchMap((selectedBcfFile) => {
            if (selectedBcfFile.fileName) {
              return this.backendService.saveBcfFile(selectedBcfFile);
            } else {
              return of(false);
            }
          })
        )
        .subscribe({
          next: (value?) => {
            if (typeof value === 'boolean' && !value) {
              this.notificationsService.info(
                'Please use "Save As" to save the BCF file.'
              );
            } else {
              this.notificationsService.success('BCF file saved successfully.');
            }
          },
          error: (error) => {
            console.error('Error exporting BCF file:', error);
            this.notificationsService.error('Failed to save BCF file.');
          },
        });

      this.bcfFilesMessengerService.bcfFileSelected
        .pipe(takeUntil(this.destroyed$))
        .subscribe((selectedBcfFile) => {
          this.bcfFilesMessengerService.bcfFiles
            .pipe(take(1))
            .subscribe((bcfFiles) => {
              if (
                bcfFiles &&
                bcfFiles.length &&
                selectedBcfFile &&
                selectedBcfFile.bcfFile
              ) {
                this.updateTabIndex(
                  selectedBcfFile.bcfFile,
                  bcfFiles.map((f) => f.bcfFile!)
                );
              }
            });
        });
    }
  }

  closeBcfFile(bcfFile: BcfFile): void {
    this.bcfFilesMessengerService.closeBcfFile(bcfFile);
    if (this.tabGroup() && this.tabGroup()?.selectedIndex !== null) {
      this.changeSelectedTabIndex(this.tabGroup()?.selectedIndex || 0);
    }
  }

  changeSelectedTabIndex(index: number): void {
    this.bcfFilesMessengerService.bcfFiles
      .pipe(take(1))
      .subscribe((bcfFiles) => {
        if (bcfFiles.length && bcfFiles[index] !== undefined) {
          this.bcfFilesMessengerService.setBcfFileSelected(bcfFiles[index]);
        }
      });
  }

  private updateTabIndex(bcfFile: BcfFile, bcfFiles: BcfFile[]): void {
    if (this.tabGroup()) {
      this.tabGroup().selectedIndex = bcfFiles.indexOf(bcfFile);
    }
  }
}
