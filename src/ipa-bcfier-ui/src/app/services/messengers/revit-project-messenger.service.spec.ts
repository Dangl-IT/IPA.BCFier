import { RevitProjectMessengerService } from './revit-project-messenger.service';
import { TestBed } from '@angular/core/testing';

describe('RevitProjectMessengerService', () => {
  let service: RevitProjectMessengerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RevitProjectMessengerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
