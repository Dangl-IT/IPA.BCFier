import { Pipe, PipeTransform } from '@angular/core';

import { BcfTopic } from '../generated-client/generated-client';

@Pipe({
  name: 'topicFilter',
  standalone: true,
})
export class TopicFilterPipe implements PipeTransform {
  transform(topics: BcfTopic[], filter: string): BcfTopic[] {
    if (!filter || filter.trim() === '') {
      return topics;
    }

    const searchWords = filter
      .toLowerCase()
      .split(/\s+/)
      .filter((word) => word);

    return topics.filter((topic) => {
      const title = topic.title || '';
      const description = topic.description || '';

      const commentTexts =
        topic.comments?.map((comment) => comment.text || '').join(' ') || '';

      const combinedText =
        `${title} ${description} ${commentTexts}`.toLowerCase();

      return searchWords.every((word) => combinedText.includes(word));
    });
  }
}
