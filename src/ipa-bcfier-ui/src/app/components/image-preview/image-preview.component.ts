import { Component, inject } from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { ViewpointImageDirective } from '../../directives/viewpoint-image.directive';
import { BcfViewpoint } from '../../generated-client/generated-client';

@Component({
    selector: 'bcfier-image-preview',
    imports: [MatDialogModule, ViewpointImageDirective],
    templateUrl: './image-preview.component.html',
    styleUrl: './image-preview.component.scss'
})
export class ImagePreviewComponent {  viewpoint = inject<BcfViewpoint>(MAT_DIALOG_DATA);

}
