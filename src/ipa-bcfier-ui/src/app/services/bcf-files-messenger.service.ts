import { BcfFile, BcfFileWrapper } from '../generated-client/generated-client';
import { ReplaySubject, Subject } from 'rxjs';

import { Injectable } from '@angular/core';
import { getNewRandomGuid } from '../functions/uuid';

@Injectable({
  providedIn: 'root',
})
export class BcfFilesMessengerService {
  private bcfFilesSubject: ReplaySubject<BcfFileWrapper[]> = new ReplaySubject<
    BcfFileWrapper[]
  >(1);
  public bcfFiles = this.bcfFilesSubject.asObservable();
  private currentBcfFiles: BcfFileWrapper[] = [];

  private bcfFileSaveAsRequestedSource = new Subject<void>();
  bcfFileSaveAsRequested = this.bcfFileSaveAsRequestedSource.asObservable();

  private bcfFileSelectedSource = new Subject<BcfFileWrapper>();
  bcfFileSelected = this.bcfFileSelectedSource.asObservable();
  createNewBcfFile(): void {
    const bcfFile: BcfFileWrapper = {
      fileName: '',
      bcfFile: {
      fileName: 'issues.bcf',
      topics: [],
      fileAttachments: [],
      project: {
        id: getNewRandomGuid(),
      },
      projectExtensions: {
        priorities: [],
        snippetTypes: [],
        topicLabels: [],
        topicStatuses: [],
        topicTypes: [],
        users: [],
        },
      },
    };
    this.currentBcfFiles.push(bcfFile);
    this.bcfFilesSubject.next(this.currentBcfFiles);
    this.bcfFileSelectedSource.next(bcfFile);
  }

  saveCurrentActiveBcfFileAs(): void {
    this.bcfFileSaveAsRequestedSource.next();
  }

  openBcfFile(bcfFileWrapper: BcfFileWrapper) {
    this.currentBcfFiles.push(bcfFileWrapper);
    this.bcfFilesSubject.next(this.currentBcfFiles);
    this.bcfFileSelectedSource.next(bcfFileWrapper);
  }

  closeBcfFile(bcfFile: BcfFile) {
    this.currentBcfFiles = this.currentBcfFiles.filter((f) => f !== bcfFile);
    this.bcfFilesSubject.next(this.currentBcfFiles);
  }
}
