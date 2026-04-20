// BUG-001 (Challenge 05): Cook Mode off-by-one — frontend observable symptom.
//
// The backend endpoint (`src/RecipeHub.Api/Endpoints/CookModeEndpoints.cs:37`)
// subtracts 1 from the 1-indexed `stepNumber`, so requesting step 1 returns
// step 0 (=404) and step N returns step N-1.
//
// The frontend (`src/RecipeHub.Web/src/hooks/useCookMode.ts:19`) initializes
// `currentStep` to 1, which IS the correct 1-indexed value — but combined
// with the backend bug, the user sees Step 1 of N render as "not found" or
// the wrong instruction, and the final step is unreachable via Next.
//
// These tests describe the CORRECT observable behavior once the backend bug
// is fixed. They're `it.skip`-ed with a BUG-001 tag; Challenge 05
// participants un-skip them, watch them fail, then fix the backend
// subtraction (and verify the hook's 1-indexed initial state stays aligned
// with the API).
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { act } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import type { ReactNode } from 'react';
import { useCookMode } from '../useCookMode';
import { apiClient, type CookModeDto } from '../../api';

function wrapper() {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={client}>{children}</QueryClientProvider>
  );
}

function makeStep(stepNumber: number, total: number, instruction: string): CookModeDto {
  return {
    recipeId: 1,
    totalSteps: total,
    stepNumber,
    instruction,
    timerMinutes: null,
  };
}

describe('useCookMode — BUG-001 planted-bug specs', () => {
  beforeEach(() => {
    vi.spyOn(apiClient, 'getCookStep').mockImplementation(
      async (_recipeId: number, stepNumber: number) => {
        // Simulate a CORRECT backend: step 1 returns the first instruction.
        const instructions = ['Preheat oven', 'Mix ingredients', 'Bake'];
        return makeStep(stepNumber, 3, instructions[stepNumber - 1] ?? '');
      },
    );
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it.skip('BUG-001: opens Cook Mode on step 1 and shows the FIRST instruction', async () => {
    const { result } = renderHook(() => useCookMode(1), { wrapper: wrapper() });

    await waitFor(() => {
      expect(result.current.step).toBeDefined();
    });

    expect(result.current.currentStep).toBe(1);
    expect(result.current.step?.stepNumber).toBe(1);
    expect(result.current.step?.instruction).toBe('Preheat oven');
  });

  it.skip('BUG-001: Next advances from step 1 to step 2 with the second instruction', async () => {
    const { result } = renderHook(() => useCookMode(1), { wrapper: wrapper() });

    await waitFor(() => expect(result.current.step?.stepNumber).toBe(1));

    act(() => {
      result.current.next();
    });

    await waitFor(() => {
      expect(result.current.currentStep).toBe(2);
      expect(result.current.step?.stepNumber).toBe(2);
      expect(result.current.step?.instruction).toBe('Mix ingredients');
    });
  });
});
