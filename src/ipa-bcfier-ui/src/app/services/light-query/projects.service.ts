import { CollectionViewer } from '@angular/cdk/collections';
import { DataSource } from '@angular/cdk/table';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { PaginationBaseService, PaginationResult } from 'ng-lightquery';
import { Observable, map, of } from 'rxjs';
import { ProjectGet } from '../../generated-client/generated-client';
@Injectable({
  providedIn: 'root',
})
export class ProjectsService
  extends PaginationBaseService<ProjectGet>
  implements DataSource<ProjectGet>
{
  constructor(override http: HttpClient) {
    super(http);
    this.baseUrl = `api/projects`;
  }

  disconnect(collectionViewer: CollectionViewer): void {
    throw new Error('Method not implemented.');
  }

  connect(): Observable<ProjectGet[]> {
    //TODO replace with paginationResult
    const projects = Array.from({ length: 100 }, (_, k) =>
      createNewProjects(k + 1)
    );
    return of(projects);
    // return this.paginationResult.pipe(
    //   map((r: PaginationResult<ProjectGet>) => r.data)
    // );
  }
}

function createNewProjects(id: number): any {
  return {
    name: `qwerty${id}`,
    created: new Date(Math.round(Math.random() * 100000)),
  };
}
