import { Injectable } from '@angular/core';
import { BehaviorSubject, ReplaySubject, filter, switchMap } from 'rxjs';
import {
  ProjectUserGet,
  ProjectUsersClient,
} from '../generated-client/generated-client';
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

  getAllUsers(): void {
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
