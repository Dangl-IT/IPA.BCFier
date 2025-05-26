import { TestBed } from '@angular/core/testing';

import { NavisworksClashesLoadingService } from './navisworks-clashes-loading.service';
import { AppTestingModule } from '../app.testing.module';

describe('NavisworksClashesLoadingService', () => {
  let service: NavisworksClashesLoadingService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AppTestingModule]
    });
    service = TestBed.inject(NavisworksClashesLoadingService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
