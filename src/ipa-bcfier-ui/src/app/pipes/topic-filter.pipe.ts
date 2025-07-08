import { Pipe, PipeTransform } from '@angular/core';

import { BcfTopic } from '../generated-client/generated-client';

@Pipe({
  name: 'topicFilter',
})
export class TopicFilterPipe implements PipeTransform {
  /**
   * Filters topics by search string and optionally by a list of allowed topic IDs.
   *
   * @param topics - The list of BcfTopics to filter
   * @param searchText - A search string (e.g., user input)
   * @param filterIds - An optional list of topic GUIDs to include
   */
  transform(
    topics: BcfTopic[],
    searchText: string,
    filterIds?: string[]
  ): BcfTopic[] {
    if (!topics) return [];

    const hasSearch = !!searchText?.trim();
    const hasIdFilter = Array.isArray(filterIds) && filterIds.length > 0;

    const searchWords = hasSearch
      ? searchText.toLowerCase().split(/\s+/).filter(Boolean)
      : [];

    return topics.filter((topic) => {
      const matchesId =
        !hasIdFilter ||
        (!!topic.serverAssignedId &&
          filterIds.includes(topic.serverAssignedId));

      const matchesText =
        !hasSearch ||
        (() => {
          const title = topic.title || '';
          const description = topic.description || '';
          const commentTexts =
            topic.comments?.map((c) => c.text || '').join(' ') || '';
          const combined =
            `${title} ${description} ${commentTexts}`.toLowerCase();
          return searchWords.every((word) => combined.includes(word));
        })();

      return matchesId && matchesText;
    });
  }
}
