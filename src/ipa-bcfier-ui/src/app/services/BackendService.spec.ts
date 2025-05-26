import { TestBed } from '@angular/core/testing';

import { BackendService } from './BackendService';
import { AppTestingModule } from '../app.testing.module';

describe('BackendService', () => {
  let service: BackendService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AppTestingModule]
    });
    service = TestBed.inject(BackendService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
