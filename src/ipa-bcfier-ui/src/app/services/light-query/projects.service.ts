import { CollectionViewer } from '@angular/cdk/collections';
import { DataSource } from '@angular/cdk/table';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { PaginationBaseService, PaginationResult } from 'ng-lightquery';
import { Observable, map, of } from 'rxjs';
import { ProjectGet } from '../../generated-client/generated-client';
import { SettingsMessengerService } from '../settings-messenger.service';
@Injectable({
  providedIn: 'root',
})
export class ProjectsService
  extends PaginationBaseService<ProjectGet>
  implements DataSource<ProjectGet>
{
  constructor(
    override http: HttpClient,
    private settingsMessengerService: SettingsMessengerService
  ) {
    super(http);
    this.baseUrl = `api/projects`;

    this.settingsMessengerService.settings.subscribe((settings) => {
      this.forceRefresh();
    });
  }

  disconnect(collectionViewer: CollectionViewer): void {
    throw new Error('Method not implemented.');
  }

  connect(): Observable<ProjectGet[]> {
    return this.paginationResult.pipe(
      map((r: PaginationResult<ProjectGet>) => r.data)
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
