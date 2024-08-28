import { TestBed } from '@angular/core/testing';

import { BcfierHubConnectorService } from './bcfier-hub-connector.service';

describe('BcfierHubConnectorService', () => {
  let service: BcfierHubConnectorService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BcfierHubConnectorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
