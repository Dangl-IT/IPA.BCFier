import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class NavisworksClashProgressMessengerService {
  public navisworksClashesTotalCount = new Subject<number>();
  public navisworksClashesCurrentCount = new Subject<number>();
  public cancelGeneration = new Subject<void>();
}
