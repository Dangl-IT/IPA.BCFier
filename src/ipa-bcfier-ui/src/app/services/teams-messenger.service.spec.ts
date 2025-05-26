import { TestBed } from '@angular/core/testing';

import { TeamsMessengerService } from './teams-messenger.service';
import { AppTestingModule } from '../app.testing.module';

describe('TeamsMessengerService', () => {
  let service: TeamsMessengerService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AppTestingModule]
    });
    service = TestBed.inject(TeamsMessengerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
