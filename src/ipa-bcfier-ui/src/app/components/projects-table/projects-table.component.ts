import {
  AfterViewInit,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  ViewChild,
  inject,
} from '@angular/core';
import { AsyncPipe, CommonModule, DatePipe } from '@angular/common';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import {
  ProjectGet,
  ProjectPost,
  ProjectPut,
  ProjectUsersClient,
  ProjectsClient,
} from '../../generated-client/generated-client';
import { combineLatestWith, filter, switchMap } from 'rxjs';

import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { NotificationsService } from '../../services/notifications.service';
import { ProjectDetailsComponent } from '../project-details/project-details.component';
import { ProjectsService } from '../../services/light-query/projects.service';
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
export class ProjectsTableComponent implements AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  projectsService = inject(ProjectsService);
  settingsMessengerService = inject(SettingsMessengerService);
  notificationsService = inject(NotificationsService);
  projectsClient = inject(ProjectsClient);
  projectUsersClient = inject(ProjectUsersClient);
  matDialog = inject(MatDialog);
  cdr = inject(ChangeDetectorRef);
  dataSource!: MatTableDataSource<ProjectGet>;
  displayedColumns = ['name', 'created', 'actions'];
  filter = '';

  constructor() {
    this.projectsService.connect().subscribe((projects) => {
      this.dataSource = new MatTableDataSource(projects);
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
  }

  ngAfterViewInit() {
    if (this.dataSource) {
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    }
  }

  applyFilter(filter: string) {
    this.projectsService.setQueryParameter(
      'filter',
      filter.trim().toLowerCase()
    );
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  openProjectDetails(project: ProjectGet): void {
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
        next: () => {
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
            .pipe(combineLatestWith(this.settingsMessengerService.settings));
        }),
        switchMap(([p, s]) =>
          this.projectUsersClient.addUserToProject(p.id, {
            identifier: s.username,
          })
        )
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
        error: () => {},
      });
  }
}
