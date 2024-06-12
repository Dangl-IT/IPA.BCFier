import { Injectable, inject } from '@angular/core';

import { BcfTopic } from '../generated-client/generated-client';

@Injectable({
  providedIn: 'root',
})
export class IssueFilterService {
  filterIssue(
    issues: BcfTopic[],
    withoutStatus: boolean,
    status: string,
    withoutType: boolean,
    type: string,
    withoutUser: boolean,
    users: string[],
    dateStart: Date | null,
    dateEnd: Date | null
  ): BcfTopic[] {
    if (!issues || !issues.length) {
      return [];
    }
    return issues.filter((issue) => {
      let passesStatus = true;
      let passesType = true;
      let passesUsers = true;
      let passesDate = true;

      if (withoutStatus) {
        passesStatus = !issue.topicStatus;
      } else if (status && issue.topicStatus !== status) {
        passesStatus = false;
      }

      if (withoutType) {
        passesType = !issue.topicType;
      } else if (type && issue.topicType !== type) {
        passesType = false;
      }

      if (withoutUser) {
        passesUsers = !issue.assignedTo;
      } else if (
        (users &&
          users.length > 0 &&
          issue.assignedTo &&
          !users.includes(issue.assignedTo)) ||
        (users.length > 0 && !issue.assignedTo)
      ) {
        passesUsers = false;
      }

      if (
        !!issue.dueDate &&
        dateStart &&
        new Date(issue.dueDate).getTime() < new Date(dateStart).getTime()
      ) {
        passesDate = false;
      }

      if (
        dateEnd &&
        !!issue.dueDate &&
        new Date(issue.dueDate).getTime() > new Date(dateEnd).getTime()
      ) {
        passesDate = false;
      }

      if ((dateStart || dateEnd) && !issue.dueDate) {
        passesDate = false;
      }
      return passesStatus && passesType && passesDate && passesUsers;
    });
  }
}
