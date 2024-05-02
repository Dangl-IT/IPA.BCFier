import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatTabsModule } from '@angular/material/tabs';
import { ProjectsTableComponent } from '../projects-table/projects-table.component';
import { SettingsClient } from '../../generated-client/generated-client';
import { SettingsMessengerService } from '../../services/settings-messenger.service';

@Component({
  selector: 'bcfier-settings',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    FormsModule,
    MatInputModule,
    MatButtonModule,
    MatTabsModule,
    ProjectsTableComponent,
  ],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss',
})
export class SettingsComponent implements OnInit, OnDestroy {
  constructor(
    private dialogRef: MatDialogRef<SettingsComponent>,
    public settingsMessengerService: SettingsMessengerService,
    private settingsClient: SettingsClient
  ) {}

  username: string = '';
  mainDatabaseSaveLocation: string = '';

  private destroyed$ = new Subject<void>();

  ngOnInit(): void {
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
