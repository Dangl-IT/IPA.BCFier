import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UsersComponent } from './users.component';
import { AppTestingModule } from '../../app.testing.module';
import { UsersService } from '../../services/light-query/users.service';

describe('UsersComponent', () => {
  let component: UsersComponent;
  let fixture: ComponentFixture<UsersComponent>;
  UsersService.prototype.disconnect = () => {};

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UsersComponent, AppTestingModule],
      providers: [
        { provide: UsersService }
      ]
    })
    .compileComponents();
    fixture = TestBed.createComponent(UsersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
