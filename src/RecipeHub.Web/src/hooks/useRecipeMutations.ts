import { useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../api';
import type {
  CreateRecipeRequest,
  RecipeDetail,
  UpdateRecipeRequest,
} from '../api';
import { recipeKeys } from './queryKeys';

export function useCreateRecipe() {
  const qc = useQueryClient();
  return useMutation<RecipeDetail, Error, CreateRecipeRequest>({
    mutationFn: (req) => apiClient.createRecipe(req),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: recipeKeys.lists() });
    },
  });
}

export function useUpdateRecipe(id: number) {
  const qc = useQueryClient();
  return useMutation<void, Error, UpdateRecipeRequest>({
    mutationFn: (req) => apiClient.updateRecipe(id, req),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: recipeKeys.lists() });
      qc.invalidateQueries({ queryKey: recipeKeys.detail(id) });
    },
  });
}

export function useDeleteRecipe() {
  const qc = useQueryClient();
  return useMutation<void, Error, number>({
    mutationFn: (id) => apiClient.deleteRecipe(id),
    onSuccess: (_data, id) => {
      qc.invalidateQueries({ queryKey: recipeKeys.lists() });
      qc.removeQueries({ queryKey: recipeKeys.detail(id) });
    },
  });
}
