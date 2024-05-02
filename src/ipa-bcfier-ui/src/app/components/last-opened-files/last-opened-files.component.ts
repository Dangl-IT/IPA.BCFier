import {
  ChangeDetectionStrategy,
  Component,
  effect,
  input,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';

@Component({
  selector: 'bcfier-last-opened-files',
  standalone: true,
  imports: [MatMenuModule, MatButtonModule],
  templateUrl: './last-opened-files.component.html',
  styleUrl: './last-opened-files.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LastOpenedFilesComponent {
  isOpen = input<boolean>(false);
  btnWidth = input<number>(0);
  //TODO replace type any
  lastOpenedFiles = input<any[]>([]);

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
}
