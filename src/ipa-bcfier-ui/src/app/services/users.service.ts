import { BehaviorSubject, ReplaySubject, filter, switchMap, take } from 'rxjs';
import {
  ProjectUserGet,
  ProjectUsersClient,
} from '../generated-client/generated-client';

import { Injectable } from '@angular/core';
import { SelectedProjectMessengerService } from './selected-project-messenger.service';

@Injectable({
  providedIn: 'root',
})
export class UsersService {
  private usersSource = new BehaviorSubject<ProjectUserGet[]>([]);
  users = this.usersSource.asObservable();
  constructor(
    private selectedProjectMessengerService: SelectedProjectMessengerService,
    private projectUsersClient: ProjectUsersClient
  ) {
    this.getAllUsers();
  }

  setUsers(users: ProjectUserGet[]): void {
    this.usersSource.next(users);
  }

  refreshUsers(): void {
    this.selectedProjectMessengerService.selectedProject
      .pipe(take(1))
      .subscribe((project) => {
        if (project) {
          this.projectUsersClient
            .getProjectUsersForProject(project.id)
            .subscribe((users) => {
              this.setUsers(users);
            });
        }
      });
  }

  private getAllUsers(): void {
    this.selectedProjectMessengerService.selectedProject
      .pipe(
        filter((p) => !!p),
        switchMap((p) => {
          return this.projectUsersClient.getProjectUsersForProject(p?.id || '');
        })
      )
      .subscribe((users) => {
        this.setUsers(users);
      });
  }
}
