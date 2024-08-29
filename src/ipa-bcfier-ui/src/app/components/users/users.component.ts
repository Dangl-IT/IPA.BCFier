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
import { AsyncPipe, CommonModule } from '@angular/common';
import {
  MatPaginator,
  MatPaginatorModule,
  PageEvent,
} from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { Subject, filter, switchMap, takeUntil } from 'rxjs';
import { UserGet, UsersClient } from '../../generated-client/generated-client';

import { AddUserComponent } from '../add-user/add-user.component';
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
import { SettingsMessengerService } from '../../services/settings-messenger.service';
import { UsersService } from '../../services/light-query/users.service';

@Component({
  selector: 'bcfier-users',
  standalone: true,
  imports: [
    CommonModule,
    MatFormFieldModule,
    MatInputModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    AsyncPipe,
    MatIconModule,
    MatButtonModule,
    FormsModule,
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsersComponent implements AfterViewInit, OnDestroy, OnInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  usersService = inject(UsersService);
  usersClient = inject(UsersClient);
  settingsMessengerService = inject(SettingsMessengerService);
  notificationsService = inject(NotificationsService);
  matDialog = inject(MatDialog);
  cdr = inject(ChangeDetectorRef);
  appConfigService = inject(AppConfigService);

  paginationResult: PaginationResult<UserGet> = {
    data: [],
    page: 1,
    pageSize: 20,
    totalCount: 0,
  };

  private destroyed$ = new Subject<void>();
  displayedColumns = ['identifier', 'actions'];
  filter = '';
  shouldEnableProjectManagement =
    this.appConfigService.shouldEnableProjectManagementFeatures();

  ngOnInit(): void {
    this.usersService.onSort({
      active: 'identifier',
      direction: 'desc',
    });
    this.usersService.paginationResult
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
    this.paginator.pageSize = this.usersService.pageSize;
  }

  applyFilter(filter: string) {
    this.usersService.setQueryParameter('filter', filter.trim().toLowerCase());
  }

  createUser(): void {
    this.matDialog
      .open(AddUserComponent, {
        autoFocus: false,
        restoreFocus: false,
        width: '80%',
        maxHeight: '70vh',
      })
      .afterClosed()
      .pipe(
        filter((newProject) => !!newProject),
        switchMap((newUserName: string) => {
          return this.usersClient.createUser(newUserName);
        })
      )
      .subscribe({
        next: () => {
          this.usersService.forceRefresh();

          if (this.filter) {
            this.applyFilter(this.filter);
          }
          this.notificationsService.success('User added');
        },
        error: () => {
          this.notificationsService.error('Failed to add user');
        },
      });
  }

  deleteUser(userId: string): void {
    this.matDialog
      .open(ConfirmDialogComponent, {
        autoFocus: false,
        restoreFocus: false,
        data: 'delete this user',
      })
      .afterClosed()
      .pipe(
        filter((confirm) => !!confirm),
        switchMap(() => this.usersClient.deleteUser(userId))
      )
      .subscribe({
        next: () => {
          this.notificationsService.success('User deleted');
          this.usersService.forceRefresh();
          if (this.filter) {
            this.applyFilter(this.filter);
          }
        },
        error: () => {
          this.notificationsService.error('Failed to delete user');
        },
      });
  }

  refresh(): void {
    this.usersService.forceRefresh();
  }

  onPage(pageEvent: PageEvent): void {
    this.usersService.page = pageEvent.pageIndex + 1;
    this.usersService.pageSize = pageEvent.pageSize;
  }
}
