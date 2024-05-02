import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
export const IssueTypes = new Set([
  'Architecture',
  'Structural',
  'Electrical',
  'HVAC',
  'W&S',
  'Fire Extinguishing',
  'Gas',
  'Technology',
]);
@Injectable({
  providedIn: 'root',
})
export class IssueTypesService {
  private issueTypesSource = new BehaviorSubject<Set<string>>(IssueTypes);
  issueTypes = this.issueTypesSource.asObservable();
  constructor() {}

  setIssueTypes(types: string | string[]): void {
    const statusArray = typeof types === 'string' ? [types] : types;

    const updatedTypes = new Set<string>([...IssueTypes, ...statusArray]);
    this.issueTypesSource.next(updatedTypes);
  }
}
