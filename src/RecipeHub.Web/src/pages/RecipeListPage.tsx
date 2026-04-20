import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Badge, Card, Spinner } from '../components/ui';
import { useRecipes } from '../hooks';
import styles from './RecipeListPage.module.css';

export function RecipeListPage() {
  const navigate = useNavigate();
  const { data, isLoading, isError, error } = useRecipes();
  // TODO: Replace this trivial title filter with the richer SearchBar in Item 15.
  const [query, setQuery] = useState('');

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();
    const list = data ?? [];
    if (!q) return list;
    return list.filter((r) => r.title.toLowerCase().includes(q));
  }, [data, query]);

  return (
    <div>
      <div className={styles.header}>
        <h1>Recipes</h1>
        <input
          type="search"
          className={styles.search}
          placeholder="Filter by title…"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          aria-label="Filter recipes by title"
        />
      </div>

      {isLoading ? (
        <Spinner label="Loading recipes…" />
      ) : isError ? (
        <div className={styles.error}>
          Couldn't load recipes. {error instanceof Error ? error.message : ''}
        </div>
      ) : filtered.length === 0 ? (
        <div className={styles.empty}>
          {query ? 'No recipes match your filter.' : 'No recipes yet.'}
        </div>
      ) : (
        <div className={styles.grid}>
          {filtered.map((r) => (
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
