import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SelectedTopicListMessengerService {
  private selectedTopicListChangedSource = new Subject<void>();
  selectedTopicListChanged = this.selectedTopicListChangedSource.asObservable();

  public notifySelectedTopicListChanged(): void {
    this.selectedTopicListChangedSource.next();
  }
}
