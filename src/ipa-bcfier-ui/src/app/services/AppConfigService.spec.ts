import { TestBed } from '@angular/core/testing';

import { AppConfigService } from './AppConfigService';
import { AppTestingModule } from '../app.testing.module';

describe('AppConfigService', () => {
  let service: AppConfigService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AppTestingModule]
    });
    service = TestBed.inject(AppConfigService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
