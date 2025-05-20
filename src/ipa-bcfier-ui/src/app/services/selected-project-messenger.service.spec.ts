import { TestBed } from '@angular/core/testing';

import { SelectedProjectMessengerService } from './selected-project-messenger.service';
import { AppTestingModule } from '../app.testing.module';

describe('SelectedProjectMessengerService', () => {
  let service: SelectedProjectMessengerService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AppTestingModule]
    });
    service = TestBed.inject(SelectedProjectMessengerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
