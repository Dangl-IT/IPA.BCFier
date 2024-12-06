import { TestBed } from '@angular/core/testing';

import { SettingsMessengerService } from './settings-messenger.service';
import { AppTestingModule } from '../app.testing.module';

describe('SettingsMessengerService', () => {
  let service: SettingsMessengerService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AppTestingModule]
    });
    service = TestBed.inject(SettingsMessengerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
