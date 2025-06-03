import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatRadioModule } from '@angular/material/radio';
import { LoadingService } from '../../services/loading.service';
import {
  BcfTopic,
  GroupingType,
  IfcGuidNamePair,
  NavisworksClashGroupingData,
  NavisworksClashSelection,
  ViewpointsClient,
} from '../../generated-client/generated-client';
import { ClashSelectComponent } from '../clash-select/clash-select.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { GroupingTypePipe } from '../../pipes/grouping-type.pipe';
import { BackendService } from '../../services/BackendService';
import { ElementSelectComponent } from '../element-select/element-select.component';

@Component({
  selector: 'bcfier-clash-grouping-options',
  imports: [
    MatButtonModule,
    FormsModule,
    MatDialogModule,
    MatRadioModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    MatInputModule,
    GroupingTypePipe,
  ],
  templateUrl: './clash-grouping-options.component.html',
  styleUrls: ['./clash-grouping-options.component.scss'],
})
export class ClashGroupingOptionsComponent implements OnInit {
  private loadingService = inject(LoadingService);
  private viewpointsClient = inject(ViewpointsClient);
  private dialogRef = inject(MatDialogRef<ClashGroupingOptionsComponent>);
  private dialogData: {
    activeTopic: BcfTopic | null;
  } = inject(MAT_DIALOG_DATA);
  private backendService = inject(BackendService);

  public selectedGroupingType = signal<GroupingType>(GroupingType.Selection);
  readonly groupingTypes: GroupingType[] = [
    GroupingType.Proximity,
    GroupingType.Level,
    GroupingType.Selection,
  ];
  public clashes: NavisworksClashSelection[] = [];
  public proximityRadius: number | null = null;
  public levelTolerance: number | null = null;
  public viewpointElements: IfcGuidNamePair[] = [];

  ngOnInit(): void {
    this.loadActiveElements();
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

  private loadActiveElements(): void {
    if (
      this.dialogData?.activeTopic &&
      this.dialogData.activeTopic.viewpoints?.length > 0
    ) {
      const selectedComponentIfcGuids =
        this.dialogData.activeTopic.viewpoints
          .map((viewpoint) => viewpoint.viewpointComponents?.selectedComponents)
          .flat()
          .map((component) => {
            return {
              ifcGuid: component.ifcGuid,
              revitId: component.authoringToolId,
              name: '',
            } as IfcGuidNamePair;
          }) || [];

      if (selectedComponentIfcGuids.length > 0) {
        this.backendService
          .getElementNamesList(selectedComponentIfcGuids)
          .subscribe({
            next: (list: IfcGuidNamePair[]) => {
              this.viewpointElements = list;
            },
            error: (error) => {
              // Just ignoring the error here
              console.error(error);
            },
          });
      }
    }
  }

  save(): void {
    const groupingData: NavisworksClashGroupingData = {
      clashId: this.dialogData?.activeTopic?.serverAssignedId,
      groupingType: this.selectedGroupingType(),
      proximityGroupingOptions: {
        radius: this.proximityRadius || undefined,
      },
      levelGroupingOptions: {
        tolerance: this.levelTolerance || undefined,
      },
    };
    this.dialogRef.close(groupingData);
  }

  close(): void {
    this.dialogRef.close();
  }

  resetInputsValue(): void {
    this.proximityRadius = null;
    this.levelTolerance = null;
  }
}
