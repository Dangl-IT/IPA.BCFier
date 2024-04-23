import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class UsersService {
  //TODO replace type any
  private usersSource = new ReplaySubject<any[]>(1);
  users = this.usersSource.asObservable();
  constructor() {
    this.getAllUsers();
  }

  //TODO replace type any
  setUsers(users: any[]): void {
    this.usersSource.next(users);
  }

  getAllUsers(): void {
    //TODO update, after add backend for getting users
    this.setUsers(['Borys', 'Georg']);
  }
}
