import {
  MAT_DATE_LOCALE,
  provideNativeDateAdapter,
} from '@angular/material/core';

import { AppConfigService } from './services/AppConfigService';
import { ApplicationConfig } from '@angular/core';
import { BackendService } from './services/BackendService';
import { IMAGE_CONFIG } from '@angular/common';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideToastr } from 'ngx-toastr';
import { routes } from './app.routes';

const frontendConfigService = new AppConfigService();

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(),
    {
      provide: BackendService,
      useClass: BackendService,
    },
    {
      provide: IMAGE_CONFIG,
      useValue: {
        disableImageSizeWarning: true,
        disableImageLazyLoadWarning: true,
      },
    },
    provideToastr({
      positionClass: 'toast-bottom-right',
      preventDuplicates: true,
      closeButton: true,
    }),
    { provide: MAT_DATE_LOCALE, useValue: 'en-UK' },
    provideNativeDateAdapter(),
  ],
};
