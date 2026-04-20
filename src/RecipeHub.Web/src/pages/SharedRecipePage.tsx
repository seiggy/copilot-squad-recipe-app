import { Link, useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Badge, Spinner } from '../components/ui';
import { apiClient } from '../api';
import type { RecipeDetail } from '../api';
import { shareKeys } from '../hooks';
import { ApiError } from '../api/client';
import styles from './SharedRecipePage.module.css';

export function SharedRecipePage() {
  const { token } = useParams<{ token: string }>();

  const { data, isLoading, error, isError } = useQuery<RecipeDetail>({
    queryKey: shareKeys.byToken(token ?? ''),
    queryFn: () => apiClient.getSharedRecipe(token as string),
    enabled: typeof token === 'string' && token.length > 0,
    retry: false,
  });

  return (
    <div className={styles.page}>
      <header className={styles.topBar}>
        <Link to="/" className={styles.brand}>
          RecipeHub
        </Link>
      </header>

      <main className={styles.main}>
        {isLoading ? (
          <Spinner label="Loading shared recipe…" />
        ) : isError && error instanceof ApiError && error.status === 404 ? (
          <div className={styles.notFound}>
            <h1>Recipe not available</h1>
            <p>This shared recipe is no longer available.</p>
            <Link to="/">Browse recipes on RecipeHub</Link>
          </div>
        ) : isError || !data ? (
          <div className={styles.error}>
            Couldn't load shared recipe.{' '}
            {error instanceof Error ? error.message : ''}
          </div>
        ) : (
          <article>
            <h1 className={styles.title}>{data.title}</h1>
            <div className={styles.meta}>
              <span>{data.difficulty}</span>
              <span>Prep {data.prepTimeMinutes}m</span>
              <span>Cook {data.cookTimeMinutes}m</span>
              <span>Serves {data.servings}</span>
            </div>
            <div className={styles.tags}>
              {data.tagNames.map((t) => (
                <Badge key={t} variant="info">
                  {t}
                </Badge>
              ))}
            </div>
            {data.description ? (
              <p className={styles.description}>{data.description}</p>
            ) : null}
            <section>
              <h2>Steps</h2>
              <ol className={styles.steps}>
                {[...data.steps]
                  .sort((a, b) => a.stepNumber - b.stepNumber)
                  .map((s) => (
                    <li key={s.stepNumber} className={styles.step}>
                      {s.instruction}
                      {s.timerMinutes != null ? (
                        <span className={styles.timer}>
                          ({s.timerMinutes} min)
                        </span>
                      ) : null}
                    </li>
                  ))}
              </ol>
            </section>
            <footer className={styles.footer}>
              <Link to="/">View on RecipeHub</Link>
            </footer>
          </article>
        )}
      </main>
    </div>
  );
}

export default SharedRecipePage;
