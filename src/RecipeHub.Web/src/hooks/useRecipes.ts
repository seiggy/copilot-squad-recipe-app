import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../api';
import type { Recipe, RecipeDetail } from '../api';
import { recipeKeys } from './queryKeys';

export function useRecipes() {
  return useQuery<Recipe[]>({
    queryKey: recipeKeys.lists(),
    queryFn: () => apiClient.listRecipes(),
  });
}

export function useRecipe(id: number | undefined) {
  return useQuery<RecipeDetail>({
    queryKey: id !== undefined ? recipeKeys.detail(id) : ['recipes', 'detail', 'disabled'],
    queryFn: () => apiClient.getRecipe(id as number),
    enabled: typeof id === 'number' && Number.isFinite(id),
  });
}
