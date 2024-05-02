import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { ProjectGet } from '../generated-client/generated-client';

@Injectable({
  providedIn: 'root',
})
export class SelectedProjectMessengerService {
  private selectedProjectSource = new ReplaySubject<ProjectGet | null>(1);
  selectedProject = this.selectedProjectSource.asObservable();

  constructor() {}

  setSelectedProject(project: ProjectGet | null): void {
    this.selectedProjectSource.next(project);
  }
}
