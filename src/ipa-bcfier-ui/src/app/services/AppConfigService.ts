import { FrontendConfig } from '../generated-client/generated-client';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AppConfigService {
  private defaultFrontendConfig: FrontendConfig = {
    isInElectronMode: false,
    isConnectedToRevit: false,
    isConnectedToNavisworks: false,
    environment: 'Production',
  };

  getFrontendConfig(): FrontendConfig {
    return (
      ((window as any)['ipaBcfierFrontendConfig'] as FrontendConfig) ||
      this.defaultFrontendConfig
    );
  }

  shouldEnableProjectManagementFeatures(): boolean {
    return (
      !this.getFrontendConfig().isConnectedToNavisworks &&
      !this.getFrontendConfig().isConnectedToRevit
    );
  }
}
