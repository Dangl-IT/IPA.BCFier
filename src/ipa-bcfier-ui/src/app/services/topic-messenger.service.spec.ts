import { TestBed } from '@angular/core/testing';

import { TopicMessengerService } from './topic-messenger.service';
import { AppTestingModule } from '../app.testing.module';

describe('TopicMessengerService', () => {
  let service: TopicMessengerService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AppTestingModule]
    });
    service = TestBed.inject(TopicMessengerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
