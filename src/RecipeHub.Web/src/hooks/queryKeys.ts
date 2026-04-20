export const recipeKeys = {
  all: ['recipes'] as const,
  lists: () => [...recipeKeys.all, 'list'] as const,
  detail: (id: number) => [...recipeKeys.all, 'detail', id] as const,
};

export const tagKeys = {
  all: ['tags'] as const,
};
