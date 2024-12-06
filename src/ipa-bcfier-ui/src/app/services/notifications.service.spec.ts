import { TestBed } from '@angular/core/testing';

import { NotificationsService } from './notifications.service';
import { AppTestingModule } from '../app.testing.module';

describe('NotificationsService', () => {
  let service: NotificationsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AppTestingModule]
    });
    service = TestBed.inject(NotificationsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
