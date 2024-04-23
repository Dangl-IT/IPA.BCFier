import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
export const IssueStatuses = new Set([
  'New',
  'Active',
  'Resolved',
  'Reviewed',
  'Approved',
]);
@Injectable({
  providedIn: 'root',
})
export class IssueStatusesService {
  private issueStatusesSource = new BehaviorSubject<Set<string>>(IssueStatuses);
  issueStatuses = this.issueStatusesSource.asObservable();
  constructor() {}

  setIssueStatuses(statuses: string | string[]): void {
    const statusArray = typeof statuses === 'string' ? [statuses] : statuses;

    const updatedStatuses = new Set<string>([...IssueStatuses, ...statusArray]);
    this.issueStatusesSource.next(updatedStatuses);
  }
}
