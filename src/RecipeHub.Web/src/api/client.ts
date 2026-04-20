import type {
  CookModeDto,
  CreateRecipeRequest,
  Recipe,
  RecipeDetail,
  ShareDto,
  Tag,
  UpdateRecipeRequest,
} from './types';

export class ApiError extends Error {
  readonly status: number;
  readonly body: unknown;

  constructor(status: number, message: string, body: unknown) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.body = body;
  }
}

function resolveBaseUrl(): string {
  const configured = import.meta.env.VITE_API_BASE_URL;
  if (configured && configured.length > 0) {
    return configured.replace(/\/$/, '');
  }
  if (import.meta.env.DEV) {
    console.warn(
      '[api] VITE_API_BASE_URL not set; falling back to http://localhost:5000'
    );
    return 'http://localhost:5000';
  }
  throw new Error('VITE_API_BASE_URL is not configured.');
}

const BASE_URL = resolveBaseUrl();

async function request<T>(
  path: string,
  init: RequestInit & { parseJson?: boolean } = {}
): Promise<T> {
  const { parseJson = true, headers, ...rest } = init;

  const response = await fetch(`${BASE_URL}${path}`, {
    credentials: 'include',
    headers: {
      Accept: 'application/json',
      ...headers,
    },
    ...rest,
  });

  if (!response.ok) {
    let body: unknown = null;
    const text = await response.text();
    if (text.length > 0) {
      try {
        body = JSON.parse(text);
      } catch {
        body = text;
      }
    }
    const message =
      (typeof body === 'object' && body !== null && 'title' in body
        ? String((body as { title: unknown }).title)
        : undefined) ?? `Request failed: ${response.status} ${response.statusText}`;
    throw new ApiError(response.status, message, body);
  }

  if (response.status === 204 || !parseJson) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

function jsonInit(method: 'POST' | 'PUT', body: unknown): RequestInit {
  return {
    method,
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  };
}

export const apiClient = {
  listRecipes: (): Promise<Recipe[]> => request<Recipe[]>('/api/recipes'),

  getRecipe: (id: number): Promise<RecipeDetail> =>
    request<RecipeDetail>(`/api/recipes/${id}`),

  createRecipe: (req: CreateRecipeRequest): Promise<RecipeDetail> =>
    request<RecipeDetail>('/api/recipes', jsonInit('POST', req)),

  updateRecipe: (id: number, req: UpdateRecipeRequest): Promise<void> =>
    request<void>(`/api/recipes/${id}`, {
      ...jsonInit('PUT', req),
      parseJson: false,
    }),

  deleteRecipe: (id: number): Promise<void> =>
    request<void>(`/api/recipes/${id}`, { method: 'DELETE', parseJson: false }),

  listTags: (): Promise<Tag[]> => request<Tag[]>('/api/tags'),

  getCookStep: (recipeId: number, stepNumber: number): Promise<CookModeDto> =>
    request<CookModeDto>(`/api/recipes/${recipeId}/cook/steps/${stepNumber}`),

  searchRecipes: (q: string, tag?: string): Promise<Recipe[]> => {
    const params = new URLSearchParams();
    params.set('q', q);
    if (tag && tag.length > 0) {
      params.set('tag', tag);
    }
    return request<Recipe[]>(`/api/recipes/search?${params.toString()}`);
  },

  shareRecipe: (recipeId: number): Promise<ShareDto> =>
    request<ShareDto>(`/api/recipes/${recipeId}/share`, { method: 'POST' }),

  getSharedRecipe: (token: string): Promise<RecipeDetail> =>
    request<RecipeDetail>(`/api/shared/${encodeURIComponent(token)}`),
};

export type ApiClient = typeof apiClient;
