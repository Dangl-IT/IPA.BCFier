import {
  AfterViewInit,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnDestroy,
  OnInit,
  ViewChild,
  inject,
} from '@angular/core';
import { AsyncPipe, CommonModule, DatePipe } from '@angular/common';
import {
  MatPaginator,
  MatPaginatorModule,
  PageEvent,
} from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import {
  ProjectGet,
  ProjectPost,
  ProjectPut,
  ProjectUsersClient,
  ProjectsClient,
} from '../../generated-client/generated-client';
import {
  Subject,
  catchError,
  combineLatestWith,
  filter,
  of,
  switchMap,
  take,
  takeUntil,
  tap,
} from 'rxjs';

import { AppConfigService } from '../../services/AppConfigService';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { NotificationsService } from '../../services/notifications.service';
import { PaginationResult } from 'ng-lightquery';
import { ProjectDetailsComponent } from '../project-details/project-details.component';
import { ProjectsService } from '../../services/light-query/projects.service';
import { SelectedProjectMessengerService } from '../../services/selected-project-messenger.service';
import { SettingsMessengerService } from '../../services/settings-messenger.service';

@Component({
  selector: 'bcfier-projects-table',
  standalone: true,
  imports: [
    CommonModule,
    MatFormFieldModule,
    MatInputModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    DatePipe,
    AsyncPipe,
    MatIconModule,
    MatButtonModule,
    FormsModule,
  ],
  templateUrl: './projects-table.component.html',
  styleUrl: './projects-table.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectsTableComponent
  implements AfterViewInit, OnDestroy, OnInit
{
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  projectsService = inject(ProjectsService);
  settingsMessengerService = inject(SettingsMessengerService);
  notificationsService = inject(NotificationsService);
  selectedProjectMessengerService = inject(SelectedProjectMessengerService);
  projectsClient = inject(ProjectsClient);
  projectUsersClient = inject(ProjectUsersClient);
  matDialog = inject(MatDialog);
  cdr = inject(ChangeDetectorRef);
  appConfigService = inject(AppConfigService);

  private destroyed$ = new Subject<void>();
  displayedColumns = ['name', 'createdAtUtc', 'actions'];
  filter = '';
  selectedProject: ProjectGet | null = null;
  shouldEnableProjectManagement =
    this.appConfigService.shouldEnableProjectManagementFeatures();
  paginationResult: PaginationResult<ProjectGet> = {
    data: [],
    page: 1,
    pageSize: 20,
    totalCount: 0,
  };

  ngOnInit(): void {
    this.selectedProjectMessengerService.selectedProject
      .pipe(takeUntil(this.destroyed$))
      .subscribe((p) => {
        this.selectedProject = p;
      });
    this.projectsService.onSort({
      active: 'createdAtUtc',
      direction: 'desc',
    });

    this.projectsService.paginationResult
      .pipe(takeUntil(this.destroyed$))
      .subscribe((paginationResult) => {
        this.paginationResult = paginationResult;
      });
  }

  ngOnDestroy(): void {
    this.destroyed$.next();
    this.destroyed$.complete();
  }

  ngAfterViewInit() {
    this.paginator.pageSize = this.projectsService.pageSize;
  }

  applyFilter(filter: string) {
    this.projectsService.setQueryParameter(
      'filter',
      filter.trim().toLowerCase()
    );
  }

  projectClicked(project: ProjectGet): void {
    if (this.shouldEnableProjectManagement) {
      this.openProjectDetails(project);
    } else {
      this.setSelectedProject(project);
    }
  }

  private openProjectDetails(project: ProjectGet): void {
    this.matDialog
      .open(ProjectDetailsComponent, {
        autoFocus: false,
        width: '80%',
        maxHeight: '70vh',
        data: project,
      })
      .afterClosed()
      .pipe(
        filter((newProject) => !!newProject),
        switchMap((editedProject: ProjectPut) => {
          return this.projectsClient.editProject(project.id, {
            ...editedProject,
            id: project.id,
          });
        })
      )
      .subscribe({
        next: (p) => {
          this.selectedProjectMessengerService.setSelectedProject(p);
          this.projectsService.forceRefresh();
          if (this.filter) {
            this.applyFilter(this.filter);
          }
          this.notificationsService.success('Project updated');
        },
        error: () => {},
      });
  }

  createProject(): void {
    this.matDialog
      .open(ProjectDetailsComponent, {
        autoFocus: false,
        restoreFocus: false,
        width: '80%',
        maxHeight: '70vh',
      })
      .afterClosed()
      .pipe(
        filter((newProject) => !!newProject),
        switchMap((newProject: ProjectPost) => {
          return this.projectsClient
            .createProject(newProject)
            .pipe(
              combineLatestWith(this.settingsMessengerService.settings),
              take(1)
            );
        }),
        switchMap(([p, s]) => {
          return this.projectUsersClient
            .addUserToProject(p.id, {
              identifier: s.username,
            })
            .pipe(
              catchError((err) => {
                this.projectsService.forceRefresh();

                if (this.filter) {
                  this.applyFilter(this.filter);
                }
                this.notificationsService.success('Project created');
                this.notificationsService.info(
                  'Current user was not added, please add manually'
                );
                return of([]);
              }),
              tap(() => {
                this.selectedProjectMessengerService.setSelectedProject(p);
              })
            );
        })
      )
      .subscribe({
        next: () => {
          this.projectsService.forceRefresh();

          if (this.filter) {
            this.applyFilter(this.filter);
          }
          this.notificationsService.success('Project created');
        },
        error: () => {},
      });
  }

  deleteProject(projectId: string): void {
    this.matDialog
      .open(ConfirmDialogComponent, {
        autoFocus: false,
        restoreFocus: false,
        data: 'delete this project',
      })
      .afterClosed()
      .pipe(
        filter((confirm) => !!confirm),
        switchMap(() => this.projectsClient.deleteProject(projectId))
      )
      .subscribe({
        next: () => {
          this.projectsService.forceRefresh();
          if (this.filter) {
            this.applyFilter(this.filter);
          }
        },
        error: () => {
          this.notificationsService.error('Failed to delete the project.');
        },
      });
  }

  setSelectedProject(p: ProjectGet): void {
    if (p.id === this.selectedProject?.id) {
      this.selectedProjectMessengerService.setSelectedProject(null);
      return;
    }
    this.selectedProjectMessengerService.setSelectedProject(p);
  }

  refresh(): void {
    this.projectsService.forceRefresh();
  }

  onPage(pageEvent: PageEvent): void {
    this.projectsService.page = pageEvent.pageIndex + 1;
    this.projectsService.pageSize = pageEvent.pageSize;
  }
}
