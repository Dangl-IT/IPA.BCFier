import { TestBed } from '@angular/core/testing';

import { ProjectUsersService } from './project-users.service';
import { AppTestingModule } from '../app.testing.module';

describe('ProjectUsersService', () => {
  let service: ProjectUsersService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AppTestingModule]
    });
    service = TestBed.inject(ProjectUsersService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
