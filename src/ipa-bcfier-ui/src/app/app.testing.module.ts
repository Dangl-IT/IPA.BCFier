import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NgModule } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { RouterModule } from '@angular/router';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ToastrModule, ToastrService } from 'ngx-toastr';
import { provideNativeDateAdapter } from '@angular/material/core';

@NgModule({
  declarations: [],
  imports: [RouterModule.forRoot([]), ToastrModule, NoopAnimationsModule],
  exports: [ToastrModule, RouterModule],
  providers: [
    { provide: MatDialogRef, useValue: { close: () => {} } },
    { provide: MAT_DIALOG_DATA, useValue: [] },
    { provide: ToastrService, useValue: {} },
    provideNativeDateAdapter(),
    provideHttpClient(),
    provideHttpClientTesting()
  ]
})
export class AppTestingModule {}
