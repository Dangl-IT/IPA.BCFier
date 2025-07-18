import {
  BcfFile,
  BcfFileWrapper,
  ProjectGet,
} from '../../generated-client/generated-client';
import {
  Component,
  OnInit,
  TemplateRef,
  inject,
  viewChild,
} from '@angular/core';
import { MatTabGroup, MatTabsModule } from '@angular/material/tabs';
import {
  Observable,
  Subject,
  catchError,
  filter,
  map,
  of,
  switchMap,
  take,
  takeUntil,
  tap,
} from 'rxjs';

import { AsyncPipe } from '@angular/common';
import { BackendService } from '../../services/BackendService';
import { BcfFileAutomaticallySaveService } from '../../services/bcf-file-automaticaly-save.service';
import { BcfFileComponent } from '../bcf-file/bcf-file.component';
import { BcfFilesMessengerService } from '../../services/bcf-files-messenger.service';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { NotificationsService } from '../../services/notifications.service';
import { ProjectsService } from '../../services/light-query/projects.service';
import { ProjectsTableComponent } from '../projects-table/projects-table.component';
import { RevitProjectMessengerService } from '../../services/messengers/revit-project-messenger.service';
import { SelectedProjectMessengerService } from '../../services/selected-project-messenger.service';

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
  revitDialogContent =
    viewChild.required<TemplateRef<unknown>>('revitDialogContent');
  private destroyed$ = new Subject<void>();
  bcfFiles!: Observable<BcfFileWrapper[]>;
  selectedProject: ProjectGet | null = null;

  private bcfFilesMessengerService = inject(BcfFilesMessengerService);
  private notificationsService = inject(NotificationsService);
  private backendService = inject(BackendService);
  private revitProjectMessengerService = inject(RevitProjectMessengerService);
  private projectsService = inject(ProjectsService);
  private selectedProjectMessengerService = inject(
    SelectedProjectMessengerService
  );
  private bcfFileAutomaticallySaveService = inject(
    BcfFileAutomaticallySaveService
  );
  private dialog = inject(MatDialog);

  ngOnDestroy(): void {
    this.destroyed$.next();
    this.destroyed$.complete();
  }

  ngOnInit(): void {
    //Here we get messages only if app is connected to Revit
    this.revitProjectMessengerService.revitProject
      .pipe(takeUntil(this.destroyed$))
      .subscribe((project) => {
        if (project) {
          this.findRevitProjectInDatabase(
            project.projectNumber,
            project.filePath
          );
        }
      });
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

  private findRevitProjectInDatabase(
    projectNumber: string,
    filePath: string
  ): void {
    this.projectsService.getAll().subscribe((projects) => {
      if (projects?.length && projects.length > 0) {
        let selectedProject =
          projects.find(
            (p) =>
              p.number === projectNumber &&
              p.revitFilePath === filePath &&
              p.number?.length > 0
          ) ||
          projects.find((p) => p.revitFilePath === filePath) ||
          projects.find(
            (p) => p.number === projectNumber && p.number?.length > 0
          );

        if (
          this.selectedProjectMessengerService.lastSelectedProjectId ===
            selectedProject?.id ||
          !selectedProject
        ) {
          // In that case, we don't want to show the dialog and just keep everything as-is
          return;
        }

        this.selectedProject = selectedProject;
        this.dialog
          .open(ConfirmDialogComponent, {
            autoFocus: false,
            restoreFocus: false,
            disableClose: true,
            data: {
              contentTemplate: this.revitDialogContent(),
              cancelBtnText: 'Switch Project',
            },
          })
          .afterClosed()
          .subscribe((confirm) => {
            if (confirm) {
              this.selectedProjectMessengerService.setSelectedProject(
                this.selectedProject
              );
            } else {
              this.dialog
                .open(ProjectsTableComponent, {
                  autoFocus: false,
                  restoreFocus: false,
                  disableClose: false,
                  panelClass: 'projects-table-dialog',
                })
                .afterClosed()
                .subscribe((project: ProjectGet) => {
                  if (project) {
                    this.selectedProjectMessengerService.setSelectedProject(
                      project
                    );
                  }
                });
            }
          });
      } else {
        this.selectedProject = null;
        this.revitProjectMessengerService.setRevitProject(this.selectedProject);
      }
    });
  }
}
