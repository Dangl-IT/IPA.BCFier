import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  Output,
  inject,
} from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
} from '@angular/forms';

import { AsyncPipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { Observable } from 'rxjs';
import { ProjectUserGet } from '../../generated-client/generated-client';
import { ProjectUsersService } from '../../services/project-users.service';

export interface IFilters {
  status: FormControl<string>;
  type: FormControl<string>;
  users: FormControl<string[]>;
  issueRange: FormGroup<{
    start: FormControl<Date | null>;
    end: FormControl<Date | null>;
  }>;
}

@Component({
  selector: 'bcfier-issue-filters',
  standalone: true,
  imports: [
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    AsyncPipe,
    ReactiveFormsModule,
    MatDatepickerModule,
  ],
  styleUrl: './issue-filters.component.scss',
  templateUrl: './issue-filters.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IssueFiltersComponent {
  @Input({
    required: true,
  })
  issueStatuses$!: Observable<Set<string>>;

  @Input({
    required: true,
  })
  issueTypes$!: Observable<Set<string>>;

  @Input({
    required: true,
  })
  users$!: Observable<ProjectUserGet[]>;
  projectUsersService = inject(ProjectUsersService);

  @Output()
  acceptedFilters = new EventEmitter<FormGroup<IFilters>>();

  fb = inject(FormBuilder);

  filtersForm: FormGroup<IFilters>;
  constructor() {
    this.filtersForm = this.fb.group({
      status: new FormControl<string>('', { nonNullable: true }),
      type: new FormControl<string>('', { nonNullable: true }),
      users: new FormControl<string[]>([], { nonNullable: true }),
      issueRange: new FormGroup({
        start: new FormControl<Date | null>(null),
        end: new FormControl<Date | null>(null),
      }),
    });
  }

  acceptFilters(): void {
    this.acceptedFilters.emit(this.filtersForm);
  }

  clearFilters(): void {
    this.filtersForm.reset();
    this.acceptedFilters.emit(this.filtersForm);
  }

  refreshUsers(): void {
    this.projectUsersService.refreshUsers();
  }
}
