import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Badge, Card, Spinner } from '../components/ui';
import { FilterPanel, SearchBar } from '../components/search';
import { useRecipes, useSearch } from '../hooks';
import type { Recipe } from '../api';
import styles from './RecipeListPage.module.css';

export function RecipeListPage() {
  const navigate = useNavigate();
  const [query, setQuery] = useState('');
  const [tag, setTag] = useState<string | undefined>(undefined);

  const hasFilters = query.trim().length > 0 || tag !== undefined;
  const allRecipes = useRecipes();
  const searchResults = useSearch({ q: query, tag });

  const active = hasFilters ? searchResults : allRecipes;
  const recipes: Recipe[] = active.data ?? [];

  return (
    <div>
      <div className={styles.header}>
        <h1>Recipes</h1>
        <SearchBar value={query} onChange={setQuery} />
      </div>
      <FilterPanel selectedTag={tag} onTagChange={setTag} />

      {active.isLoading ? (
        <Spinner label="Loading recipes…" />
      ) : active.isError ? (
        <div className={styles.error}>
          Couldn't load recipes.{' '}
          {active.error instanceof Error ? active.error.message : ''}
        </div>
      ) : recipes.length === 0 ? (
        <div className={styles.empty}>
          {hasFilters ? 'No recipes match your filters.' : 'No recipes yet.'}
        </div>
      ) : (
        <div className={styles.grid}>
          {recipes.map((r) => (
            <Card
              key={r.id}
              title={r.title}
              onClick={() => navigate(`/recipes/${r.id}`)}
            >
              <p className={styles.description}>
                {r.description ?? 'No description.'}
              </p>
              <div className={styles.tags}>
                {r.tagNames.map((t) => (
                  <Badge key={t} variant="info">
                    {t}
                  </Badge>
                ))}
              </div>
              <div className={styles.meta}>
                <span>{r.difficulty}</span>
                <span>
                  Prep {r.prepTimeMinutes}m · Cook {r.cookTimeMinutes}m
                </span>
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}

export default RecipeListPage;
