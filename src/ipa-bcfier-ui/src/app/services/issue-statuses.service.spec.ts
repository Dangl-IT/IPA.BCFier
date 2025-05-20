import { TestBed } from '@angular/core/testing';

import { IssueStatusesService } from './issue-statuses.service';
import { AppTestingModule } from '../app.testing.module';

describe('IssueStatusesService', () => {
  let service: IssueStatusesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AppTestingModule]
    });
    service = TestBed.inject(IssueStatusesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
