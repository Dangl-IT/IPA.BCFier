import { TestBed } from '@angular/core/testing';

import { IssueTypesService } from './issue-types.service';
import { AppTestingModule } from '../app.testing.module';

describe('IssueTypesService', () => {
  let service: IssueTypesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AppTestingModule]
    });
    service = TestBed.inject(IssueTypesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
