import { TitleCasePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatRadioModule } from '@angular/material/radio';
import { LoadingService } from '../../services/loading.service';
import {
  NavisworksClashSelection,
  ViewpointsClient,
} from '../../generated-client/generated-client';
import { ClashSelectComponent } from '../clash-select/clash-select.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
type GroupingType = 'proximity' | 'level' | 'selection' | '';

@Component({
  selector: 'bcfier-clash-grouping-options',
  imports: [
    MatButtonModule,
    FormsModule,
    MatDialogModule,
    MatRadioModule,
    TitleCasePipe,
    ClashSelectComponent,
    MatFormFieldModule,
    ReactiveFormsModule,
    MatInputModule,
  ],
  templateUrl: './clash-grouping-options.component.html',
  styleUrl: './clash-grouping-options.component.scss',
})
export class ClashGroupingOptionsComponent implements OnInit {
  private loadingService = inject(LoadingService);
  private viewpointsClient = inject(ViewpointsClient);
  private dialogRef = inject(MatDialogRef<ClashGroupingOptionsComponent>);

  public selectedGroupingType = signal<GroupingType>('');
  readonly groupingTypes: GroupingType[] = ['proximity', 'level', 'selection'];
  public clashes: NavisworksClashSelection[] = [];
  public selectedClashIds: string[] = [];
  public proximityRadius: number | null = null;
  public levelTolerance: number | null = null;
  ngOnInit(): void {
    this.viewpointsClient.getAvailableNavisworksClashes().subscribe({
      next: (clashes) => {
        this.loadingService.hideLoadingScreen();
        this.clashes = clashes;
      },
      error: () => {
        this.loadingService.hideLoadingScreen();
      },
    });
  }
  save(): void {
    this.dialogRef.close({
      checkType: this.selectedGroupingType,
      proximityOptions: {
        baseClashIds: this.selectedClashIds,
        distance: this.proximityRadius,
      },
      levelOptions: {
        baseClashCheckId: this.selectedClashIds,
        distance: this.levelTolerance,
      },
      selectionOptions: {
        baseClashCheckId: this.selectedClashIds,
      },
    });
  }

  close(): void {
    this.dialogRef.close();
  }

  onSelectedIdsChange(selectedIds: string[]): void {
    this.resetInputsValue();
    this.selectedClashIds = selectedIds;
  }

  resetInputsValue(): void {
    this.proximityRadius = null;
    this.levelTolerance = null;
  }
}
