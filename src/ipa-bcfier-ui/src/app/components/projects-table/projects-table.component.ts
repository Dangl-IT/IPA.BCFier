import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  ViewChild,
  inject,
} from '@angular/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ProjectsService } from '../../services/light-query/projects.service';
import { AsyncPipe, DatePipe } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { ProjectDetailsComponent } from '../project-details/project-details.component';
@Component({
  selector: 'bcfier-projects-table',
  standalone: true,
  imports: [
    MatFormFieldModule,
    MatInputModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    DatePipe,
    AsyncPipe,
  ],
  templateUrl: './projects-table.component.html',
  styleUrl: './projects-table.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectsTableComponent implements AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  projectsService = inject(ProjectsService);
  matDialog = inject(MatDialog);
  //TODO replace type any
  dataSource!: MatTableDataSource<any>;
  //TODO show more columns
  displayedColumns: string[] = ['name', 'created'];

  constructor() {
    this.projectsService.connect().subscribe((projects) => {
      this.dataSource = new MatTableDataSource(projects);
    });
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  //TODO replace type any
  openProjectDetails(project: any): void {
    this.matDialog.open(ProjectDetailsComponent, {
      autoFocus: false,
      width: '80%',
      maxHeight: '70vh',
      data: project,
    });
  }
}
