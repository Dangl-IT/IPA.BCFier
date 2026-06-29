import { Component, OnDestroy, OnInit, inject, ChangeDetectionStrategy } from '@angular/core';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';

import { AppConfigService } from '../../services/AppConfigService';
import { CommonModule } from '@angular/common';
import { ErrorLogsComponent } from '../error-logs/error-logs.component';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatTabsModule } from '@angular/material/tabs';
import { ProjectsTableComponent } from '../projects-table/projects-table.component';
import { SettingsClient } from '../../generated-client/generated-client';
import { SettingsMessengerService } from '../../services/settings-messenger.service';
import { UsersComponent } from '../users/users.component';

@Component({
  selector: 'bcfier-settings',
  imports: [
    CommonModule,
    MatDialogModule,
    FormsModule,
    MatInputModule,
    MatButtonModule,
    MatTabsModule,
    ErrorLogsComponent,
    ProjectsTableComponent,
    UsersComponent,
  ],
  templateUrl: './settings.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './settings.component.scss',
})
export class SettingsComponent implements OnInit, OnDestroy {
  private dialogRef = inject<MatDialogRef<SettingsComponent>>(MatDialogRef);
  settingsMessengerService = inject(SettingsMessengerService);
  private settingsClient = inject(SettingsClient);
  private appConfigService = inject(AppConfigService);

  username: string = '';
  mainDatabaseSaveLocation: string = '';
  public isInAdminMode = false;
  public isInNavisworksMode =
    this.appConfigService.getFrontendConfig().isConnectedToNavisworks;

  private destroyed$ = new Subject<void>();

  ngOnInit(): void {
    this.isInAdminMode =
      this.appConfigService.shouldEnableProjectManagementFeatures();
    this.settingsMessengerService.settings
      .pipe(takeUntil(this.destroyed$))
      .subscribe((settings) => {
        this.username = settings.username;
        this.mainDatabaseSaveLocation = settings.mainDatabaseLocation || '';
      });
  }

  ngOnDestroy(): void {
    this.destroyed$.next();
    this.destroyed$.complete();
  }

  save(): void {
    this.settingsMessengerService.saveSettings({
      username: this.username,
      mainDatabaseLocation: this.mainDatabaseSaveLocation,
    });
    this.close();
  }

  close(): void {
    this.dialogRef.close();
  }

  changeMainDatabaseSaveLocation(): void {
    this.settingsClient.choseMainDatabaseLocation().subscribe(() => {
      this.settingsMessengerService.refreshSettings();
    });
  }
}
