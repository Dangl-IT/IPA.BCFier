import { BehaviorSubject } from 'rxjs';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class GroupedClasheIdsMessengerService {
  private groupedClashIdsSource = new BehaviorSubject<string[]>([]);
  groupedClashIds = this.groupedClashIdsSource.asObservable();

  setGroupedClasheIds(ids: string[]): void {
    this.groupedClashIdsSource.next(ids);
  }

  resetGroupedClasheIds(): void {
    this.groupedClashIdsSource.next([]);
  }
}
