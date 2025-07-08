import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  Inject,
  OnDestroy,
  OnInit,
  inject,
} from '@angular/core';
import {
  FormBuilder,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import {
  MAT_DIALOG_DATA,
  MatDialog,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Observable, Subject, tap } from 'rxjs';
import { MatListModule } from '@angular/material/list';
import { AsyncPipe } from '@angular/common';
import {
  ProjectGet,
  ProjectsClient,
  ProjectUserGet,
  ProjectUsersClient,
  UserGet,
  UsersClient,
} from '../../generated-client/generated-client';
import { MatButtonModule } from '@angular/material/button';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';
import { ProjectUsersService } from '../../services/project-users.service';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { NotificationsService } from '../../services/notifications.service';
@Component({
    selector: 'bcfier-project-details',
    imports: [
        MatFormFieldModule,
        MatInputModule,
        FormsModule,
        MatListModule,
        AsyncPipe,
        ReactiveFormsModule,
        MatDialogModule,
        MatButtonModule,
        MatExpansionModule,
        MatIconModule,
        MatAutocompleteModule,
    ],
    templateUrl: './project-details.component.html',
    styleUrl: './project-details.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProjectDetailsComponent implements OnInit, OnDestroy {
  users$: Observable<ProjectUserGet[]> | null = null;
  projectDetailsForm = this.fb.group({
    name: ['', Validators.required],
    number: [''],
    teamsWebhook: [''],
    bcfFilesFolder: [{ value: '', disabled: true }],
    revitFilePath: [{ value: '', disabled: true }],
  });
  panelOpenState = false;
  identifier = '';
  private allUsers$ = inject(UsersClient).getAllUsers();
  filteredUsers$ = new Subject<UserGet[]>();
  private notificationsService = inject(NotificationsService);
  constructor(
    public dialogRef: MatDialogRef<ProjectDetailsComponent>,
    @Inject(MAT_DIALOG_DATA)
    public data: ProjectGet,
    private projectUsersClient: ProjectUsersClient,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private matDialog: MatDialog,
    private projectUsersService: ProjectUsersService,
    private projectsClient: ProjectsClient
  ) {}

  ngOnInit(): void {
    if (this.data) {
      this.projectDetailsForm.patchValue({
        name: this.data.name,
        teamsWebhook: this.data?.teamsWebhook,
        number: this.data?.number,
        bcfFilesFolder: this.data?.bcfFilesFolder,
        revitFilePath: this.data?.revitFilePath,
      });
      this.users$ = this.getProjectUsers(this.data.id);
    }
    this.filterUsers();
  }

  filterUsers(): void {
    this.allUsers$.subscribe((users) => {
      if (this.identifier) {
        this.filteredUsers$.next(
          users.filter(
            (u) =>
              u.identifier
                .toLowerCase()
                .indexOf(this.identifier.toLowerCase()) !== -1
          )
        );
      } else {
        this.filteredUsers$.next(users);
      }
    });
  }

  ngOnDestroy(): void {
    this.filteredUsers$.complete();
  }

  getProjectUsers(projectId: string): Observable<ProjectUserGet[]> {
    return this.projectUsersClient.getProjectUsersForProject(projectId);
  }

  updateProjectDetails(isUpdate: boolean): void {
    if (!isUpdate) {
      this.dialogRef.close();
      return;
    }
    const formData = this.projectDetailsForm.getRawValue();
    this.dialogRef.close(formData);
  }

  addUserToProject(): void {
    this.users$ = this.projectUsersClient
      .addUserToProject(this.data.id, {
        identifier: this.identifier,
      })
      .pipe(
        tap({
          next: (u) => {
            this.notificationsService.success('User added');
            this.projectUsersService.setUsers(u);
            this.identifier = '';
            this.filterUsers();
            this.cdr.detectChanges();
          },
          error: () => {
            this.notificationsService.error('Failed to add the user');
            this.users$ = this.getProjectUsers(this.data.id);
            this.filterUsers();
            this.cdr.detectChanges();
          },
        })
      );
  }

  deleteProjectUser(userId: string): void {
    this.matDialog
      .open(ConfirmDialogComponent, {
        autoFocus: false,
        restoreFocus: false,
        data: { action: 'delete' },
      })
      .afterClosed()
      .subscribe((confirm) => {
        if (confirm) {
          this.users$ = this.projectUsersClient
            .deleteProjectUser(this.data.id, userId)
            .pipe(tap((u) => this.projectUsersService.setUsers(u)));
          this.cdr.detectChanges();
        }
      });
  }

  chooseFolderForStorageBCFFiles(): void {
    this.projectsClient.choseBcfFilesFolderLocation().subscribe((path) => {
      this.projectDetailsForm.get('bcfFilesFolder')?.patchValue(path);
    });
  }

  chooseRevitProjectFile(): void {
    this.projectsClient.choseRevitProjectFileLocation().subscribe((path) => {
      this.projectDetailsForm.get('revitFilePath')?.patchValue(path);
    });
  }
}
