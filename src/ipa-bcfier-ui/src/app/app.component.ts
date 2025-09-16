import { Component, inject } from '@angular/core';

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
  private notificationsService = inject(NotificationsService);
  private bcfierHubConnectorService = inject(BcfierHubConnectorService);

  constructor() {
    const appConfigService = inject(AppConfigService);

    const cadPluginVersion =
      appConfigService.getFrontendConfig().cadPluginVersion;
    if (!!cadPluginVersion && version.version !== cadPluginVersion) {
      this.notificationsService.info(
        `The BCFier version (${version.version}) is different from the CAD plugin version (${cadPluginVersion}).`
      );
    }
  }
}
