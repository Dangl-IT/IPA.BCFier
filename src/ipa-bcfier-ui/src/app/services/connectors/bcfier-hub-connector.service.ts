import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
} from '@microsoft/signalr';
import { Injectable, NgZone } from '@angular/core';

import { CadErrorDialogComponent } from '../../components/cad-error-dialog/cad-error-dialog.component';
import { LoadingService } from '../loading.service';
import { MatDialog } from '@angular/material/dialog';
import { NavisworksClashProgressMessengerService } from '../messengers/navisworks-clash-progress-messenger.service';
import { NotificationsService } from '../notifications.service';

@Injectable({
  providedIn: 'root',
})
export class BcfierHubConnectorService {
  private connection: HubConnection;

  constructor(
    private notificationsService: NotificationsService,
    private ngZone: NgZone,
    private loadingService: LoadingService,
    private matDialog: MatDialog,
    private navisworksClashProgressMessengerService: NavisworksClashProgressMessengerService
  ) {
    this.connection = new HubConnectionBuilder()
      .withAutomaticReconnect()
      .withUrl(window.location.origin + '/hubs/bcfier')
      .build();

    if (this.connection.state !== HubConnectionState.Connected) {
      this.ngZone.runOutsideAngular(() => {
        this.connection.start();
      });
    }

    this.setUpMessageListeners();
  }

  private setUpMessageListeners(): void {
    this.connection.on('InternalError', (errorMessage: string) => {
      this.ngZone.run(() => {
        this.notificationsService.error(errorMessage, 'CAD Error');
        this.matDialog.open(CadErrorDialogComponent, {
          data: errorMessage,
        });
        // We usually want to hide the loading screen if we receive an error from the CAD
        // system, since that means something went wrong and waiting further is pointless
        this.loadingService.hideLoadingScreen();
      });
    });

    this.connection.on(
      'NavisworksClashIssuesTotalCount',
      (totalCount: number) => {
        this.ngZone.run(() => {
          this.navisworksClashProgressMessengerService.navisworksClashesTotalCount.next(
            totalCount
          );
        });
      }
    );

    this.connection.on(
      'NavisworksClashIssuesCurrentCount',
      (currentCount: number) => {
        this.ngZone.run(() => {
          this.navisworksClashProgressMessengerService.navisworksClashesCurrentCount.next(
            currentCount
          );
        });
      }
    );
  }
}
