import { TestBed } from '@angular/core/testing';

import { BcfierHubConnectorService } from './bcfier-hub-connector.service';
import { AppTestingModule } from '../../app.testing.module';
import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';

describe('BcfierHubConnectorService', () => {
  let service: BcfierHubConnectorService;

  beforeEach(() => {
    spyOn(HubConnectionBuilder.prototype, 'withAutomaticReconnect')
      .and.callThrough();

    spyOn(HubConnectionBuilder.prototype, 'withUrl')
      .and.callThrough();

    spyOn(HubConnectionBuilder.prototype, 'build')
      .and.returnValue({
        state: HubConnectionState.Disconnected,
        start: jasmine.createSpy().and.returnValue(Promise.resolve()),
        stop: jasmine.createSpy(),
        on: jasmine.createSpy(),
      } as any);
  });

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AppTestingModule],
    });
    service = TestBed.inject(BcfierHubConnectorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
