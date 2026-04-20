import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../api';
import type { Tag } from '../api';
import { tagKeys } from './queryKeys';

export function useTags() {
  return useQuery<Tag[]>({
    queryKey: tagKeys.all,
    queryFn: () => apiClient.listTags(),
  });
}
