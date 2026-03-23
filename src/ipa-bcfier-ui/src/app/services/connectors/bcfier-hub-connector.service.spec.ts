import { TestBed } from '@angular/core/testing';

import { BcfierHubConnectorService } from './bcfier-hub-connector.service';
import { AppTestingModule } from '../../app.testing.module';
import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { vi } from 'vitest';

describe('BcfierHubConnectorService', () => {
  let service: BcfierHubConnectorService;

  beforeEach(() => {
    vi.spyOn(HubConnectionBuilder.prototype, 'withAutomaticReconnect');

    vi.spyOn(HubConnectionBuilder.prototype, 'withUrl');

    vi.spyOn(HubConnectionBuilder.prototype, 'build').mockReturnValue({
      state: HubConnectionState.Disconnected,
      start: vi.fn().mockReturnValue(Promise.resolve()),
      stop: vi.fn(),
      on: vi.fn(),
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
