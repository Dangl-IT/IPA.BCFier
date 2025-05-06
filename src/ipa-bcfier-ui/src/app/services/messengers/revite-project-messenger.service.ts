import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ReviteProjectMessengerService {
  private reviteProjectSource = new BehaviorSubject<{
    projectNumber: string;
    filePath: string;
  } | null>(null);
  reviteProject = this.reviteProjectSource.asObservable();

  setReviteProject(
    revitProjectData: {
      projectNumber: string;
      filePath: string;
    } | null
  ): void {
    this.reviteProjectSource.next(revitProjectData);
  }
}
