// Types mirror server DTOs in src/RecipeHub.Api/Dtos/*.cs.
// Server uses System.Text.Json defaults (camelCase) — no custom naming policy in Program.cs.

export type Difficulty = 'Easy' | 'Medium' | 'Hard';

export interface Recipe {
  id: number;
  title: string;
  description: string | null;
  difficulty: Difficulty;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  imageUrl: string | null;
  tagNames: string[];
}

export interface RecipeStep {
  stepNumber: number;
  instruction: string;
  timerMinutes: number | null;
}

export interface RecipeDetail extends Recipe {
  steps: RecipeStep[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateRecipeRequest {
  title: string;
  description: string | null;
  difficulty: Difficulty;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  imageUrl: string | null;
  tags: string[];
  steps: RecipeStep[];
}

export type UpdateRecipeRequest = CreateRecipeRequest;

export interface Tag {
  id: number;
  name: string;
  recipeCount: number;
}

export interface CookModeDto {
  recipeId: number;
  totalSteps: number;
  stepNumber: number;
  instruction: string;
  timerMinutes: number | null;
}

export interface ShareDto {
  token: string;
  url: string;
}
