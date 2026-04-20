export const recipeKeys = {
  all: ['recipes'] as const,
  lists: () => [...recipeKeys.all, 'list'] as const,
  detail: (id: number) => [...recipeKeys.all, 'detail', id] as const,
  cookStep: (id: number, step: number) =>
    [...recipeKeys.all, 'cook', id, step] as const,
  search: (q: string, tag: string | undefined) =>
    [...recipeKeys.all, 'search', q, tag ?? ''] as const,
};

export const tagKeys = {
  all: ['tags'] as const,
};

export const shareKeys = {
  byToken: (token: string) => ['shared', token] as const,
};
