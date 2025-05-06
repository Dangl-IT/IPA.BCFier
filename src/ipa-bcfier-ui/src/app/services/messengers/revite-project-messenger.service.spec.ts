import { TestBed } from '@angular/core/testing';

import { ReviteProjectMessengerService } from './revite-project-messenger.service';

describe('ReviteProjectMessengerService', () => {
  let service: ReviteProjectMessengerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ReviteProjectMessengerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
