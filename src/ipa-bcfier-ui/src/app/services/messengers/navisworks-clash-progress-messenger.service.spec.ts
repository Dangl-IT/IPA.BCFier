import { TestBed } from '@angular/core/testing';

import { NavisworksClashProgressMessengerService } from './navisworks-clash-progress-messenger.service';

describe('NavisworksClashProgressMessengerService', () => {
  let service: NavisworksClashProgressMessengerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NavisworksClashProgressMessengerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
