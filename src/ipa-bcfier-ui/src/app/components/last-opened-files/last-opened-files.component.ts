import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  input,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { LastOpenedFileGet } from '../../generated-client/generated-client';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FileNamePipe } from '../../pipes/file-name.pipe';
import { BcfFilesMessengerService } from '../../services/bcf-files-messenger.service';
import { take } from 'rxjs';
import { mapToCanActivate } from '@angular/router';
import { BackendService } from '../../services/BackendService';
import { NotificationsService } from '../../services/notifications.service';

@Component({
  selector: 'bcfier-last-opened-files',
  standalone: true,
  imports: [MatMenuModule, MatButtonModule, MatTooltipModule, FileNamePipe],
  templateUrl: './last-opened-files.component.html',
  styleUrl: './last-opened-files.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LastOpenedFilesComponent {
  isOpen = input<boolean>(false);
  btnWidth = input<number>(0);
  lastOpenedFiles = input<LastOpenedFileGet[]>([]);
  private bcfFilesMessengerService = inject(BcfFilesMessengerService);
  private backendService = inject(BackendService);
  private notificationsService = inject(NotificationsService);

  constructor() {
    effect(() => {
      this.setButtonWidth();
    });
  }

  setButtonWidth(): void {
    if (this.isOpen()) {
      const panel = <HTMLElement>(
        document.getElementsByClassName('mat-mdc-menu-panel')[0]
      );
      if (panel) {
        panel.style.width = this.btnWidth() + 'px';
      }
    }
  }

  openFile(file: LastOpenedFileGet): void {
    this.bcfFilesMessengerService.bcfFiles
      .pipe(take(1))
      .subscribe((bcfFiles) => {
        const matchingFile = bcfFiles.find((f) => f.fileName === file.fileName);
        if (matchingFile) {
          this.bcfFilesMessengerService.setBcfFileSelected(matchingFile);
        } else {
          this.backendService.importBcfFile(file.fileName).subscribe({
            next: (bcfFileWrapper) => {
              this.bcfFilesMessengerService.openBcfFile(bcfFileWrapper);
            },
            error: () => {
              this.notificationsService.error(
                file.fileName,
                'Could not open file'
              );
            },
          });
        }
      });
  }
}
