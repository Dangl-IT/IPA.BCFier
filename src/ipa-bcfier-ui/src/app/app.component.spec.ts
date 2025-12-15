import { TestBed } from '@angular/core/testing';
import { AppComponent } from './app.component';
import { AppTestingModule } from './app.testing.module';
import { BcfierHubConnectorService } from './services/connectors/bcfier-hub-connector.service';

describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent, AppTestingModule],
      providers: [{ provide: BcfierHubConnectorService, useValue: {} }],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

});
