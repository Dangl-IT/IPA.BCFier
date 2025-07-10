import { TestBed } from '@angular/core/testing';

import { SelectedTopicListMessengerService } from './selected-topic-list.messenger.service';

describe('SelectedTopicListMessengerService', () => {
  let service: SelectedTopicListMessengerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SelectedTopicListMessengerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
