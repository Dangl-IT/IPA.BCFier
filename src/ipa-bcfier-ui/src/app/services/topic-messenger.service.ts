import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { BcfTopic } from '../generated-client/generated-client';

@Injectable({
  providedIn: 'root',
})
export class TopicMessengerService {
  private selectedTopicSource = new ReplaySubject<BcfTopic | null>(1);
  selectedTopic = this.selectedTopicSource.asObservable();
  constructor() {}

  setSelectedTopic(selectedTopic: BcfTopic | null): void {
    this.selectedTopicSource.next(selectedTopic);
  }
}
