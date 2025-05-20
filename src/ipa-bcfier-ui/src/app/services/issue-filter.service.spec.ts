import { TestBed } from '@angular/core/testing';

import { IssueFilterService } from './issue-filter.service';
import { AppTestingModule } from '../app.testing.module';

describe('IssueFilterService', () => {
  let service: IssueFilterService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AppTestingModule]
    });
    service = TestBed.inject(IssueFilterService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
