import { Component } from '@angular/core';

import { AppConfigService } from './services/AppConfigService';

import { BcfierHubConnectorService } from './services/connectors/bcfier-hub-connector.service';

import { MatToolbarModule } from '@angular/material/toolbar';
import { NotificationsService } from './services/notifications.service';
import { TopMenuComponent } from './components/top-menu/top-menu.component';
import { version } from './version';
import { FilesWrapperComponent } from './components/files-wrapper/files-wrapper.component';

@Component({
  selector: 'bcfier-root',
  standalone: true,
  imports: [MatToolbarModule, TopMenuComponent, FilesWrapperComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent {
  constructor(
    private notificationsService: NotificationsService,
    private bcfierHubConnectorService: BcfierHubConnectorService, // We want to initialize it so it's listening to SignalR messages
    appConfigService: AppConfigService
  ) {
    const cadPluginVersion =
      appConfigService.getFrontendConfig().cadPluginVersion;
    if (!!cadPluginVersion && version.version !== cadPluginVersion) {
      this.notificationsService.info(
        `The BCFier version (${version.version}) is different from the CAD plugin version (${cadPluginVersion}).`
      );
    }
  }
}
