import { BehaviorSubject } from 'rxjs';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class RevitProjectMessengerService {
  private revitProjectSource = new BehaviorSubject<{
    projectNumber: string;
    filePath: string;
  } | null>(null);
  revitProject = this.revitProjectSource.asObservable();

  setRevitProject(
    revitProjectData: {
      projectNumber: string;
      filePath: string;
    } | null
  ): void {
    this.revitProjectSource.next(revitProjectData);
  }
}
