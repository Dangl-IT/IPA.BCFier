import { BehaviorSubject } from 'rxjs';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class RevitProjectMessengerService {
  private revitProjectSource = new BehaviorSubject<{
    projectNumber: string | null;
    filePath: string;
  } | null>(null);
  revitProject = this.revitProjectSource.asObservable();

  setRevitProject(
    revitProjectData: {
      projectNumber: string | null;
      filePath: string;
    } | null
  ): void {
    if (revitProjectData && !revitProjectData.projectNumber) {
      revitProjectData.projectNumber = this.extractProjectNumber(
        revitProjectData.filePath
      );
    }

    this.revitProjectSource.next(revitProjectData);
  }

  private extractProjectNumber(revitFileName: string | null): string | null {
    if (revitFileName) {
      const fileName = revitFileName.split('/').pop() || '';
      const match = fileName.match(/^(\d+)-/);
      if (match && match[1]) {
        return match[1];
      }
    }

    return null;
  }
}
