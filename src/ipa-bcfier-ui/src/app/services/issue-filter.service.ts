import { Injectable, inject } from '@angular/core';
import { BcfTopic } from '../../generated/models';

@Injectable({
  providedIn: 'root',
})
export class IssueFilterService {
  filterIssue(
    issues: BcfTopic[],
    status: string,
    type: string,
    users: any,
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

      if (status && issue.topicStatus !== status) {
        passesStatus = false;
      }

      if (type && issue.topicType !== type) {
        passesType = false;
      }

      //TODO add filter by user
      // if (users && users.length > 0 && !users.includes(issue.user)) {
      //   passesUsers = false;
      // }

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
