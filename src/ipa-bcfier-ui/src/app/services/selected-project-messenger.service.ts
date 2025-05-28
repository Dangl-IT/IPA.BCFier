import { BehaviorSubject } from 'rxjs';
import { Injectable } from '@angular/core';
import { ProjectGet } from '../generated-client/generated-client';

@Injectable({
  providedIn: 'root',
})
export class SelectedProjectMessengerService {
  private selectedProjectSource = new BehaviorSubject<ProjectGet | null>(null);
  selectedProject = this.selectedProjectSource.asObservable();

  lastSelectedProjectId: string | null = null;

  constructor() {}

  setSelectedProject(project: ProjectGet | null): void {
    this.selectedProjectSource.next(project);
    this.lastSelectedProjectId = project ? project.id : null;
  }
}
