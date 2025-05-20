import { TestBed } from '@angular/core/testing';
import { SafeUrlPipe } from './safe-url.pipe';

describe('SafeUrlPipe', () => {
  it('create an instance', () => {
    TestBed.runInInjectionContext(() => {
      const pipe = new SafeUrlPipe();
      expect(pipe).toBeTruthy();
    });
  });
});
