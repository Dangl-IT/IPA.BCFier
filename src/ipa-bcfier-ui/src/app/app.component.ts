import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { filter, take } from 'rxjs';

import { AppConfigService } from './services/AppConfigService';
import { BcfierHubConnectorService } from './services/connectors/bcfier-hub-connector.service';
import { FilesWrapperComponent } from './components/files-wrapper/files-wrapper.component';
import { MatToolbarModule } from '@angular/material/toolbar';
import { NotificationsService } from './services/notifications.service';
import { ProjectsClient } from './generated-client/generated-client';
import { RevitProjectMessengerService } from './services/messengers/revit-project-messenger.service';
import { TopMenuComponent } from './components/top-menu/top-menu.component';
import { version } from './version';

@Component({
  selector: 'bcfier-root',
  standalone: true,
  imports: [MatToolbarModule, TopMenuComponent, FilesWrapperComponent],
  templateUrl: './app.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './app.component.scss',
})
export class AppComponent {
  private notificationsService = inject(NotificationsService);
  private bcfierHubConnectorService = inject(BcfierHubConnectorService);
  private projectsClient = inject(ProjectsClient);
  private revitProjectMessengerService = inject(RevitProjectMessengerService);

  constructor() {
    const appConfigService = inject(AppConfigService);

    // We're just trying to get the Revit project data after start, since Revit only
    // sends the data itself on project change or first plugin start
    if (appConfigService.getFrontendConfig().isConnectedToRevit) {
      let hasReceivedRevitProject = false;
      this.revitProjectMessengerService.revitProject
        .pipe(
          filter((p) => p != null),
          take(1)
        )
        .subscribe((projectData) => {
          hasReceivedRevitProject = true;
        });

      let hasRequestRunning = false;
      const checkForRevitProjectData = () => {
        if (!hasReceivedRevitProject) {
          if (!hasRequestRunning) {
            hasRequestRunning = true;
            this.projectsClient.refreshProjectData().subscribe({
              next: (p) => {
                hasRequestRunning = false;
              },
              error: () => {
                hasRequestRunning = false;
              },
            });
          }
          setTimeout(() => {
            checkForRevitProjectData();
          }, 1000);
        }
      };

      checkForRevitProjectData();
    }

    const cadPluginVersion =
      appConfigService.getFrontendConfig().cadPluginVersion;
    if (!!cadPluginVersion && version.version !== cadPluginVersion) {
      this.notificationsService.info(
        `The BCFier version (${version.version}) is different from the CAD plugin version (${cadPluginVersion}).`
      );
    }
  }
}
