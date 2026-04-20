import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../api';
import type { Recipe } from '../api';
import { recipeKeys } from './queryKeys';

export interface UseSearchArgs {
  q: string;
  tag?: string;
}

export function useSearch({ q, tag }: UseSearchArgs) {
  const trimmed = q.trim();
  const tagValue = tag && tag.length > 0 ? tag : undefined;
  const enabled = trimmed.length > 0 || tagValue !== undefined;

  return useQuery<Recipe[]>({
    queryKey: recipeKeys.search(trimmed, tagValue),
    queryFn: () => apiClient.searchRecipes(trimmed, tagValue),
    enabled,
  });
}
