import { Link, useNavigate, useParams } from 'react-router-dom';
import { Badge, Button, Spinner } from '../components/ui';
import { ShareButton } from '../components/recipe';
import { useDeleteRecipe, useRecipe } from '../hooks';
import styles from './RecipeDetailPage.module.css';

export function RecipeDetailPage() {
  const { id } = useParams<{ id: string }>();
  const numericId = id ? Number.parseInt(id, 10) : undefined;
  const navigate = useNavigate();
  const { data, isLoading, isError, error } = useRecipe(numericId);
  const deleteMutation = useDeleteRecipe();

  if (isLoading) {
    return <Spinner label="Loading recipe…" />;
  }

  if (isError || !data || numericId === undefined) {
    return (
      <div className={styles.error}>
        Couldn't load recipe. {error instanceof Error ? error.message : ''}
      </div>
    );
  }

  const handleDelete = () => {
    if (!window.confirm(`Delete "${data.title}"? This cannot be undone.`)) {
      return;
    }
    deleteMutation.mutate(numericId, {
      onSuccess: () => navigate('/recipes'),
    });
  };

  const sortedSteps = [...data.steps].sort((a, b) => a.stepNumber - b.stepNumber);

  return (
    <article>
      <header className={styles.header}>
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
      </header>

      {data.description ? (
        <p className={styles.description}>{data.description}</p>
      ) : null}

      <section>
        <h2>Steps</h2>
        <ol className={styles.steps}>
          {sortedSteps.map((s) => (
            <li key={s.stepNumber} className={styles.step}>
              {s.instruction}
              {s.timerMinutes != null ? (
                <span className={styles.timer}>({s.timerMinutes} min)</span>
              ) : null}
            </li>
          ))}
        </ol>
      </section>

      <div className={styles.actions}>
        <Link to={`/recipes/${numericId}/edit`}>
          <Button variant="primary">Edit</Button>
        </Link>
        <Link to={`/recipes/${numericId}/cook`}>
          <Button variant="secondary">Cook Mode</Button>
        </Link>
        <ShareButton recipeId={numericId} />
        <Button
          variant="danger"
          onClick={handleDelete}
          loading={deleteMutation.isPending}
        >
          Delete
        </Button>
      </div>
    </article>
  );
}

export default RecipeDetailPage;
