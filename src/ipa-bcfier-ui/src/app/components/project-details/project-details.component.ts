import { ChangeDetectionStrategy, Component, Inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Observable, of } from 'rxjs';
import { MatListModule } from '@angular/material/list';
import { AsyncPipe } from '@angular/common';
import {
  ProjectGet,
  ProjectUserGet,
  ProjectUsersClient,
} from '../../generated-client/generated-client';

@Component({
  selector: 'bcfier-project-details',
  standalone: true,
  imports: [
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    MatListModule,
    AsyncPipe,
  ],
  templateUrl: './project-details.component.html',
  styleUrl: './project-details.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectDetailsComponent {
  users$: Observable<ProjectUserGet[]>;
  constructor(
    public dialogRef: MatDialogRef<ProjectDetailsComponent>,
    @Inject(MAT_DIALOG_DATA)
    public data: ProjectGet,
    private projectUsersClient: ProjectUsersClient
  ) {
    this.users$ = this.getProjectUsers(data.id);
  }

  getProjectUsers(projectId: string): Observable<ProjectUserGet[]> {
    return this.projectUsersClient.getProjectUsersForProject(projectId);
  }
}
