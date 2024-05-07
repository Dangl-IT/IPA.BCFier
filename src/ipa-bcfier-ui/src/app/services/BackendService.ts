import {
  BcfConversionClient,
  BcfFileWrapper,
  BcfViewpoint,
  DocumentationClient,
  Settings,
  SettingsClient,
  ViewpointsClient,
} from '../generated-client/generated-client';
import { Observable, Subject, catchError, of, tap } from 'rxjs';

import { AddSnapshotViewpointComponent } from '../components/add-snapshot-viewpoint/add-snapshot-viewpoint.component';
import { AppConfigService } from './AppConfigService';
import { Injectable } from '@angular/core';
import { LoadingService } from './loading.service';
import { MatDialog } from '@angular/material/dialog';
import { NotificationsService } from './notifications.service';

@Injectable({
  providedIn: 'root',
})
export class BackendService {
  constructor(
    private matDialog: MatDialog,
    private appConfigService: AppConfigService,
    private loadingService: LoadingService,
    private notificationsService: NotificationsService,
    private bcfConversionClient: BcfConversionClient,
    private documentationClient: DocumentationClient,
    private settingsClient: SettingsClient,
    private viewpointsClient: ViewpointsClient
  ) {}

  importBcfFile(fileName?: string): Observable<BcfFileWrapper> {
    return this.bcfConversionClient.importBcfFile(fileName);
  }

  exportBcfFile(bcfFile: BcfFileWrapper): Observable<BcfFileWrapper> {
    return this.bcfConversionClient.exportBcfFile(bcfFile.bcfFile!);
  }

  saveBcfFile(bcfFileWrapper: BcfFileWrapper): Observable<any> {
    return this.bcfConversionClient.saveBcfFile(bcfFileWrapper);
  }

  openDocumentation(): void {
    this.documentationClient.openDocumentation().subscribe(() => {
      /* Not doing anything with the result */
    });
  }

  getSettings(): Observable<Settings> {
    return this.settingsClient.getSettings();
  }

  saveSettings(settings: Settings): Observable<void> {
    return this.settingsClient.saveSettings(settings);
  }

  addViewpoint(): Observable<BcfViewpoint | null> {
    if (
      this.appConfigService.getFrontendConfig().isConnectedToRevit ||
      this.appConfigService.getFrontendConfig().isConnectedToNavisworks
    ) {
      this.loadingService.showLoadingScreen();

      return this.viewpointsClient.createViewpoint().pipe(
        tap(() => this.loadingService.hideLoadingScreen()),
        catchError(() => {
          this.loadingService.hideLoadingScreen();
          this.notificationsService.error('Failed to add viewpoint.');
          return of(null);
        })
      );
    } else {
      const subject = new Subject<BcfViewpoint | null>();
      this.matDialog
        .open(AddSnapshotViewpointComponent)
        .afterClosed()
        .subscribe((viewpoint) => {
          if (viewpoint) {
            subject.next(viewpoint);
          } else {
            subject.next(null);
          }
          setTimeout(() => {
            subject.complete();
          }, 0);
        });

      return subject.asObservable();
    }
  }

  selectViewpoint(viewpoint: BcfViewpoint): void {
    if (
      this.appConfigService.getFrontendConfig().isConnectedToRevit ||
      this.appConfigService.getFrontendConfig().isConnectedToNavisworks
    ) {
      this.loadingService.showLoadingScreen();
      this.viewpointsClient.showViewpoint(viewpoint).subscribe({
        next: () => this.loadingService.hideLoadingScreen(),
        error: () => {
          this.loadingService.hideLoadingScreen();
          this.notificationsService.error('Failed to select viewpoint.');
        },
      });
    } else {
      // Not doing anything in the standalone version
    }
  }
}
