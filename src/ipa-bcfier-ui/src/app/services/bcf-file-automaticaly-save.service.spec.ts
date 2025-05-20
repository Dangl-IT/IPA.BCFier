import { TestBed } from '@angular/core/testing';

import { BcfFileAutomaticallySaveService } from './bcf-file-automaticaly-save.service';
import { AppTestingModule } from '../app.testing.module';

describe('BcfFileAutomaticallySaveService', () => {
  let service: BcfFileAutomaticallySaveService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AppTestingModule]
    });
    service = TestBed.inject(BcfFileAutomaticallySaveService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
