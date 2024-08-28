import {
  BcfFile,
  BcfFileWrapper,
  ProjectsClient,
} from './generated-client/generated-client';
import { Component, OnDestroy, ViewChild } from '@angular/core';
import { MatTabGroup, MatTabsModule } from '@angular/material/tabs';
import {
  Observable,
  Subject,
  filter,
  map,
  of,
  switchMap,
  take,
  takeUntil,
  tap,
} from 'rxjs';

import { AppConfigService } from './services/AppConfigService';
import { BackendService } from './services/BackendService';
import { BcfFileAutomaticallySaveService } from './services/bcf-file-automaticaly-save.service';
import { BcfFileComponent } from './components/bcf-file/bcf-file.component';
import { BcfFilesMessengerService } from './services/bcf-files-messenger.service';
import { BcfierHubConnectorService } from './services/connectors/bcfier-hub-connector.service';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { NotificationsService } from './services/notifications.service';
import { SelectedProjectMessengerService } from './services/selected-project-messenger.service';
import { TopMenuComponent } from './components/top-menu/top-menu.component';
import { version } from './version';

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
    private bcfFileAutomaticallySaveService: BcfFileAutomaticallySaveService,
    private bcfierHubConnectorService: BcfierHubConnectorService, // We want to initialize it so it's listening to SignalR messages
    appConfigService: AppConfigService,
    projectsClient: ProjectsClient,
    selectedProjectMessengerService: SelectedProjectMessengerService
  ) {
    if (
      appConfigService.getFrontendConfig().isConnectedToRevit &&
      !!appConfigService.getFrontendConfig().revitProjectPath
    ) {
      projectsClient
        .getAllProjects(
          null,
          appConfigService.getFrontendConfig().revitProjectPath
        )
        .subscribe((projects) => {
          if (projects?.data?.length && projects.data.length > 0) {
            const selectedProject = projects.data[0];
            selectedProjectMessengerService.setSelectedProject(selectedProject);
          }
        });
    }

    const cadPluginVersion =
      appConfigService.getFrontendConfig().cadPluginVersion;
    if (!!cadPluginVersion && version.version !== cadPluginVersion) {
      this.notificationsService.info(
        `The BCFier version (${version.version}) is different from the CAD plugin version (${cadPluginVersion}).`
      );
    }

    this.changeSelectedTabIndex(0);
    this.bcfFiles = bcfFilesMessengerService.bcfFiles;
    this.bcfFilesMessengerService.bcfFileSaveAsRequested
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
          return this.backendService.exportBcfFile(selectedBcfFile).pipe(
            tap((response) => {
              if (response && response.fileName) {
                selectedBcfFile.fileName = response.fileName;
                if (selectedBcfFile.bcfFile) {
                  selectedBcfFile.bcfFile.fileName = response.fileName.replace(
                    /^.*[\\/]/,
                    ''
                  );
                }
              }
            })
          );
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
    if (this.tabGroup && this.tabGroup.selectedIndex !== null) {
      this.changeSelectedTabIndex(this.tabGroup.selectedIndex);
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
}
