import { Pipe, PipeTransform } from '@angular/core';

import { GroupingType } from '../generated-client/generated-client';

@Pipe({
  name: 'groupingType',
})
export class GroupingTypePipe implements PipeTransform {
  transform(value: GroupingType): string | null {
    switch (value) {
      case GroupingType.Proximity:
        return 'Proximity';
      case GroupingType.Level:
        return 'Level';
      case GroupingType.Selection:
        return 'Selection';
      default:
        return null;
    }

    return null;
  }
}
