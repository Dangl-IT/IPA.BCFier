import { JsonPipe } from '@angular/common';

import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { ErrorLogsClient } from '../../generated-client/generated-client';
import { MatButtonModule } from '@angular/material/button';
import { NotificationsService } from '../../services/notifications.service';

@Component({
  selector: 'bcfier-error-logs',
  imports: [JsonPipe, MatButtonModule],
  templateUrl: './error-logs.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './error-logs.component.scss',
})
export class ErrorLogsComponent {
  private errorLogsClient = inject(ErrorLogsClient);
  private notificationsService = inject(NotificationsService);

  errorLogs: string | null = null;

  constructor() {
    const errorLogsClient = this.errorLogsClient;

    errorLogsClient
      .getErrorLog()
      .subscribe((errorLogs) => (this.errorLogs = errorLogs));
  }

  saveErrorLogs() {
    // Here, we want to save the error logs as string to the clipboard
    const errorLogs =
      typeof this.errorLogs == 'string'
        ? this.errorLogs
        : JSON.stringify(this.errorLogs, null, 2);
    navigator.clipboard.writeText(errorLogs);
    this.notificationsService.success(
      'Error logs copied to clipboard',
      'Success'
    );
  }
}
