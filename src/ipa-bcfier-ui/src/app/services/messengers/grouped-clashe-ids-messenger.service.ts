import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class GroupedClasheIdsMessengerService {
  private groupedClacheIdsSource = new BehaviorSubject<string[]>([]);
  groupedClacheIds = this.groupedClacheIdsSource.asObservable();

  setGroupedClasheIds(ids: string[]): void {
    this.groupedClacheIdsSource.next(ids);
  }

  resetGroupedClasheIds(): void {
    this.groupedClacheIdsSource.next([]);
  }
}
