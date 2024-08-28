import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';

import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { NavisworksClashProgressMessengerService } from '../../services/messengers/navisworks-clash-progress-messenger.service';

@Component({
  selector: 'bcfier-navisworks-clashes-loading-screen',
  standalone: true,
  imports: [MatProgressSpinnerModule, MatDialogModule, MatButtonModule],
  templateUrl: './navisworks-clashes-loading-screen.component.html',
  styleUrl: './navisworks-clashes-loading-screen.component.scss',
})
export class NavisworksClashesLoadingScreenComponent
  implements OnDestroy, OnInit
{
  constructor(
    private navisworksClashProgressMessengerService: NavisworksClashProgressMessengerService
  ) {}

  private $destroy = new Subject<void>();

  totalCount = 0;
  currentCount = 0;
  currentProgress = 0;

  ngOnInit(): void {
    this.navisworksClashProgressMessengerService.navisworksClashesTotalCount
      .pipe(takeUntil(this.$destroy))
      .subscribe((totalCount) => {
        this.totalCount = totalCount;
        this.calculateProgress();
      });

    this.navisworksClashProgressMessengerService.navisworksClashesCurrentCount
      .pipe(takeUntil(this.$destroy))
      .subscribe((currentCount) => {
        if (currentCount > this.currentCount) {
          this.currentCount = currentCount;
          this.calculateProgress();
        }
      });
  }

  private calculateProgress(): void {
    if (this.totalCount === 0) {
      this.currentProgress = 0;
      return;
    }

    const currentProgress = (100 * this.currentCount) / this.totalCount;
    if (currentProgress > this.currentProgress) {
      //  We don't want to go backwards for the progress bar if we
      // receive earlier messages later
      this.currentProgress = currentProgress;
    }
  }

  ngOnDestroy(): void {
    this.$destroy.next();
    this.$destroy.complete();
  }

  cancelGeneration(): void {
    this.navisworksClashProgressMessengerService.cancelGeneration.next();
  }
}
