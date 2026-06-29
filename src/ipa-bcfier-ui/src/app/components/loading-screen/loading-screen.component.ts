import { Component, ChangeDetectionStrategy } from '@angular/core';
import { MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
    selector: 'bcfier-loading-screen',
    imports: [MatProgressSpinnerModule, MatDialogModule],
    templateUrl: './loading-screen.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrl: './loading-screen.component.scss'
})
export class LoadingScreenComponent {}
