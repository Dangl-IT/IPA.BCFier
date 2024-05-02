import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { ProjectGet } from '../generated-client/generated-client';

@Injectable({
  providedIn: 'root',
})
export class SelectedProjectMessengerService {
  private selectedProjectSource = new BehaviorSubject<ProjectGet | null>(null);
  selectedProject = this.selectedProjectSource.asObservable();

  constructor() {}

  setSelectedProject(project: ProjectGet | null): void {
    this.selectedProjectSource.next(project);
  }
}
