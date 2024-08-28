import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class BcfFileAutomaticallySaveService {
  private bcfFileSaveAutomaticallyRequestedSource = new Subject<void>();
  bcfFileSaveAutomaticallyRequested =
    this.bcfFileSaveAutomaticallyRequestedSource.asObservable();

  constructor() {}

  saveCurrentActiveBcfFileAutomatically(): void {
    this.bcfFileSaveAutomaticallyRequestedSource.next();
  }
}
