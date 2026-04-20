import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Button, Spinner } from '../components/ui';
import type { CreateRecipeRequest, Difficulty, RecipeStep } from '../api';
import {
  useCreateRecipe,
  useRecipe,
  useTags,
  useUpdateRecipe,
} from '../hooks';
import styles from './RecipeEditPage.module.css';

type StepDraft = {
  instruction: string;
  timerMinutes: string;
};

type FormState = {
  title: string;
  description: string;
  difficulty: Difficulty;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  imageUrl: string;
  tags: string[];
  steps: StepDraft[];
};

const emptyForm: FormState = {
  title: '',
  description: '',
  difficulty: 'Easy',
  prepTimeMinutes: 0,
  cookTimeMinutes: 0,
  servings: 1,
  imageUrl: '',
  tags: [],
  steps: [{ instruction: '', timerMinutes: '' }],
};

function toStepDrafts(steps: RecipeStep[]): StepDraft[] {
  return [...steps]
    .sort((a, b) => a.stepNumber - b.stepNumber)
    .map((s) => ({
      instruction: s.instruction,
      timerMinutes: s.timerMinutes != null ? String(s.timerMinutes) : '',
    }));
}

function buildRequest(form: FormState): CreateRecipeRequest {
  const steps: RecipeStep[] = form.steps.map((s, idx) => ({
    stepNumber: idx + 1,
    instruction: s.instruction.trim(),
    timerMinutes:
      s.timerMinutes.trim() === '' ? null : Number.parseInt(s.timerMinutes, 10),
  }));

  return {
    title: form.title.trim(),
    description: form.description.trim() === '' ? null : form.description.trim(),
    difficulty: form.difficulty,
    prepTimeMinutes: form.prepTimeMinutes,
    cookTimeMinutes: form.cookTimeMinutes,
    servings: form.servings,
    imageUrl: form.imageUrl.trim() === '' ? null : form.imageUrl.trim(),
    tags: form.tags,
    steps,
  };
}

export function RecipeEditPage() {
  const { id } = useParams<{ id: string }>();
  const numericId = id ? Number.parseInt(id, 10) : undefined;
  const isEdit = typeof numericId === 'number' && Number.isFinite(numericId);
  const navigate = useNavigate();

  const recipeQuery = useRecipe(isEdit ? numericId : undefined);
  const tagsQuery = useTags();
  const createMutation = useCreateRecipe();
  const updateMutation = useUpdateRecipe(numericId ?? 0);

  const [form, setForm] = useState<FormState>(emptyForm);
  const [validationErrors, setValidationErrors] = useState<{
    title?: string;
    steps?: string;
  }>({});
  const [submitError, setSubmitError] = useState<string | null>(null);

  useEffect(() => {
    if (isEdit && recipeQuery.data) {
      const r = recipeQuery.data;
      setForm({
        title: r.title,
        description: r.description ?? '',
        difficulty: r.difficulty,
        prepTimeMinutes: r.prepTimeMinutes,
        cookTimeMinutes: r.cookTimeMinutes,
        servings: r.servings,
        imageUrl: r.imageUrl ?? '',
        tags: r.tagNames,
        steps: toStepDrafts(r.steps),
      });
    }
  }, [isEdit, recipeQuery.data]);

  const availableTags = useMemo(() => tagsQuery.data ?? [], [tagsQuery.data]);

  if (isEdit && recipeQuery.isLoading) {
    return <Spinner label="Loading recipe…" />;
  }

  const updateStep = (index: number, patch: Partial<StepDraft>) => {
    setForm((f) => ({
      ...f,
      steps: f.steps.map((s, i) => (i === index ? { ...s, ...patch } : s)),
    }));
  };

  const addStep = () =>
    setForm((f) => ({
      ...f,
      steps: [...f.steps, { instruction: '', timerMinutes: '' }],
    }));

  const removeStep = (index: number) =>
    setForm((f) => ({
      ...f,
      steps: f.steps.filter((_, i) => i !== index),
    }));

  const moveStep = (index: number, direction: -1 | 1) => {
    setForm((f) => {
      const target = index + direction;
      if (target < 0 || target >= f.steps.length) return f;
      const next = [...f.steps];
      [next[index], next[target]] = [next[target], next[index]];
      return { ...f, steps: next };
    });
  };

  const toggleTag = (name: string) => {
    setForm((f) => ({
      ...f,
      tags: f.tags.includes(name)
        ? f.tags.filter((t) => t !== name)
        : [...f.tags, name],
    }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitError(null);

    const errors: { title?: string; steps?: string } = {};
    if (form.title.trim() === '') {
      errors.title = 'Title is required.';
    }
    const nonEmptySteps = form.steps.filter((s) => s.instruction.trim() !== '');
    if (nonEmptySteps.length === 0) {
      errors.steps = 'At least one step is required.';
    }
    setValidationErrors(errors);
    if (Object.keys(errors).length > 0) return;

    const normalized: FormState = { ...form, steps: nonEmptySteps };
    const req = buildRequest(normalized);

    if (isEdit && numericId !== undefined) {
      updateMutation.mutate(req, {
        onSuccess: () => navigate(`/recipes/${numericId}`),
        onError: (err) =>
          setSubmitError(err instanceof Error ? err.message : 'Update failed.'),
      });
    } else {
      createMutation.mutate(req, {
        onSuccess: (created) => navigate(`/recipes/${created.id}`),
        onError: (err) =>
          setSubmitError(err instanceof Error ? err.message : 'Create failed.'),
      });
    }
  };

  const submitting = createMutation.isPending || updateMutation.isPending;

  return (
    <div>
      <h1>{isEdit ? 'Edit Recipe' : 'New Recipe'}</h1>
      <form className={styles.form} onSubmit={handleSubmit} noValidate>
        <div className={styles.field}>
          <label htmlFor="title">Title *</label>
          <input
            id="title"
            type="text"
            value={form.title}
            onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
          />
          {validationErrors.title ? (
            <span className={styles.error}>{validationErrors.title}</span>
          ) : null}
        </div>

        <div className={styles.field}>
          <label htmlFor="description">Description</label>
          <textarea
            id="description"
            value={form.description}
            onChange={(e) =>
              setForm((f) => ({ ...f, description: e.target.value }))
            }
          />
        </div>

        <div className={styles.row}>
          <div className={styles.field}>
            <label htmlFor="difficulty">Difficulty</label>
            <select
              id="difficulty"
              value={form.difficulty}
              onChange={(e) =>
                setForm((f) => ({
                  ...f,
                  difficulty: e.target.value as Difficulty,
                }))
              }
            >
              <option value="Easy">Easy</option>
              <option value="Medium">Medium</option>
              <option value="Hard">Hard</option>
            </select>
          </div>

          <div className={styles.field}>
            <label htmlFor="prep">Prep (min)</label>
            <input
              id="prep"
              type="number"
              min={0}
              value={form.prepTimeMinutes}
              onChange={(e) =>
                setForm((f) => ({
                  ...f,
                  prepTimeMinutes: Number.parseInt(e.target.value, 10) || 0,
                }))
              }
            />
          </div>

          <div className={styles.field}>
            <label htmlFor="cook">Cook (min)</label>
            <input
              id="cook"
              type="number"
              min={0}
              value={form.cookTimeMinutes}
              onChange={(e) =>
                setForm((f) => ({
                  ...f,
                  cookTimeMinutes: Number.parseInt(e.target.value, 10) || 0,
                }))
              }
            />
          </div>

          <div className={styles.field}>
            <label htmlFor="servings">Servings</label>
            <input
              id="servings"
              type="number"
              min={1}
              value={form.servings}
              onChange={(e) =>
                setForm((f) => ({
                  ...f,
                  servings: Number.parseInt(e.target.value, 10) || 1,
                }))
              }
            />
          </div>
        </div>

        <div className={styles.field}>
          <label htmlFor="imageUrl">Image URL</label>
          <input
            id="imageUrl"
            type="url"
            value={form.imageUrl}
            onChange={(e) =>
              setForm((f) => ({ ...f, imageUrl: e.target.value }))
            }
          />
        </div>

        <div className={styles.field}>
          <label>Tags</label>
          <div className={styles.tags}>
            {tagsQuery.isLoading ? (
              <Spinner size="sm" label="Loading tags…" />
            ) : availableTags.length === 0 ? (
              <span>No tags available.</span>
            ) : (
              availableTags.map((t) => (
                <label key={t.id} className={styles.tagCheck}>
                  <input
                    type="checkbox"
                    checked={form.tags.includes(t.name)}
                    onChange={() => toggleTag(t.name)}
                  />
                  {t.name}
                </label>
              ))
            )}
          </div>
        </div>

        <div className={styles.field}>
          <label>Steps *</label>
          <div className={styles.steps}>
            {form.steps.map((step, idx) => (
              <div key={idx} className={styles.stepRow}>
                <span className={styles.stepNumber}>{idx + 1}.</span>
                <input
                  type="text"
                  placeholder="Instruction"
                  value={step.instruction}
                  onChange={(e) =>
                    updateStep(idx, { instruction: e.target.value })
                  }
                />
                <input
                  type="number"
                  min={0}
                  placeholder="Timer (min)"
                  value={step.timerMinutes}
                  onChange={(e) =>
                    updateStep(idx, { timerMinutes: e.target.value })
                  }
                />
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  onClick={() => moveStep(idx, -1)}
                  disabled={idx === 0}
                >
                  ↑
                </Button>
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  onClick={() => moveStep(idx, 1)}
                  disabled={idx === form.steps.length - 1}
                >
                  ↓
                </Button>
                <Button
                  type="button"
                  variant="danger"
                  size="sm"
                  onClick={() => removeStep(idx)}
                  disabled={form.steps.length === 1}
                >
                  ✕
                </Button>
              </div>
            ))}
          </div>
          <div>
            <Button type="button" variant="ghost" size="sm" onClick={addStep}>
              + Add step
            </Button>
          </div>
          {validationErrors.steps ? (
            <span className={styles.error}>{validationErrors.steps}</span>
          ) : null}
        </div>

        {submitError ? (
          <div className={styles.submitError}>{submitError}</div>
        ) : null}

        <div className={styles.actions}>
          <Button type="submit" variant="primary" loading={submitting}>
            {isEdit ? 'Save Changes' : 'Create Recipe'}
          </Button>
          <Button
            type="button"
            variant="ghost"
            onClick={() => navigate('/recipes')}
          >
            Cancel
          </Button>
        </div>
      </form>
    </div>
  );
}

export default RecipeEditPage;
