import { CollectionViewer } from '@angular/cdk/collections';
import { DataSource } from '@angular/cdk/table';
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { PaginationBaseService, PaginationResult } from 'ng-lightquery';
import { Observable, map } from 'rxjs';
import { UserGet } from '../../generated-client/generated-client';
import { SettingsMessengerService } from '../settings-messenger.service';
@Injectable({
  providedIn: 'root',
})
export class UsersService
  extends PaginationBaseService<UserGet>
  implements DataSource<UserGet>
{
  private settingsMessengerService = inject(SettingsMessengerService);

  constructor() {
    const http = inject(HttpClient);

    super(http);
    this.baseUrl = `api/users`;

    this.settingsMessengerService.settings.subscribe((settings) => {
      this.forceRefresh();
    });
  }

  disconnect(collectionViewer: CollectionViewer): void {}

  connect(): Observable<UserGet[]> {
    return this.paginationResult.pipe(
      map((r: PaginationResult<UserGet>) => r.data)
    );
  }

  onSort(event: { active: string; direction: string }): void {
    if (!event.direction) {
      this.sort = null;
    } else {
      this.sort = {
        propertyName: event.active,
        isDescending: event.direction === 'desc',
      };
    }
  }
}
