import { TestBed } from '@angular/core/testing';

import { BcfierHubConnectorService } from './bcfier-hub-connector.service';
import { AppTestingModule } from '../../app.testing.module';

describe('BcfierHubConnectorService', () => {
  let service: BcfierHubConnectorService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AppTestingModule]
    });
    service = TestBed.inject(BcfierHubConnectorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
