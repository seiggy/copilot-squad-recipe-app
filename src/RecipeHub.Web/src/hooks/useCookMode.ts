import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../api';
import type { CookModeDto } from '../api';
import { recipeKeys } from './queryKeys';

export interface UseCookModeResult {
  step: CookModeDto | undefined;
  isLoading: boolean;
  error: Error | null;
  next: () => void;
  prev: () => void;
  canNext: boolean;
  canPrev: boolean;
  currentStep: number;
}

export function useCookMode(recipeId: number): UseCookModeResult {
  const [currentStep, setCurrentStep] = useState(1);

  const query = useQuery<CookModeDto>({
    queryKey: recipeKeys.cookStep(recipeId, currentStep),
    queryFn: () => apiClient.getCookStep(recipeId, currentStep),
    enabled: Number.isFinite(recipeId) && recipeId > 0,
  });

  const totalSteps = query.data?.totalSteps ?? 0;
  const canPrev = currentStep > 1;
  const canNext = totalSteps > 0 && currentStep < totalSteps;

  const next = () => {
    if (totalSteps > 0 && currentStep < totalSteps) {
      setCurrentStep((s) => s + 1);
    }
  };

  const prev = () => {
    if (currentStep > 1) {
      setCurrentStep((s) => s - 1);
    }
  };

  return {
    step: query.data,
    isLoading: query.isLoading,
    error: query.error instanceof Error ? query.error : null,
    next,
    prev,
    canNext,
    canPrev,
    currentStep,
  };
}
