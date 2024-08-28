import { Pipe, PipeTransform } from '@angular/core';

import { BcfComment } from '../generated-client/generated-client';

@Pipe({
  name: 'commentsViewpointFilter',
  standalone: true,
})
export class CommentsViewpointFilterPipe implements PipeTransform {
  transform(
    value: BcfComment[],
    viewpointId?: string,
    showAll?: boolean
  ): BcfComment[] {
    if (showAll === true) {
      return value;
    }

    const filteredComments = value.filter((comment) =>
      viewpointId ? comment.viewpointId === viewpointId : !comment.viewpointId
    );

    return filteredComments;
  }
}
