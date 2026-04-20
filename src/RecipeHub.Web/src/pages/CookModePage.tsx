import { Link, useParams } from 'react-router-dom';
import { Button, Spinner } from '../components/ui';
import { useCookMode, useTimer } from '../hooks';
import styles from './CookModePage.module.css';

function formatSeconds(total: number): string {
  const m = Math.floor(total / 60);
  const s = total % 60;
  return `${m}:${s.toString().padStart(2, '0')}`;
}

function StepTimer({ minutes }: { minutes: number }) {
  const { remainingSeconds, isRunning, start, pause, reset } = useTimer(minutes);
  return (
    <div className={styles.timer}>
      <span className={styles.timerDisplay} aria-live="polite">
        {formatSeconds(remainingSeconds)}
      </span>
      <div className={styles.timerControls}>
        {isRunning ? (
          <Button size="sm" variant="secondary" onClick={pause}>
            Pause
          </Button>
        ) : (
          <Button
            size="sm"
            variant="primary"
            onClick={start}
            disabled={remainingSeconds === 0}
          >
            Start
          </Button>
        )}
        <Button size="sm" variant="ghost" onClick={reset}>
          Reset
        </Button>
      </div>
    </div>
  );
}

export function CookModePage() {
  const { id } = useParams<{ id: string }>();
  const numericId = id ? Number.parseInt(id, 10) : Number.NaN;
  const { step, isLoading, error, next, prev, canNext, canPrev, currentStep } =
    useCookMode(numericId);

  if (!Number.isFinite(numericId)) {
    return <div className={styles.error}>Invalid recipe.</div>;
  }

  if (isLoading && !step) {
    return <Spinner label="Loading step…" />;
  }

  if (error || !step) {
    return (
      <div className={styles.error}>
        Couldn't load cook mode. {error instanceof Error ? error.message : ''}
        <div>
          <Link to={`/recipes/${numericId}`}>Back to recipe</Link>
        </div>
      </div>
    );
  }

  return (
    <article className={styles.page}>
      <header className={styles.header}>
        <Link to={`/recipes/${numericId}`} className={styles.back}>
          ← Back to recipe
        </Link>
        <h1 className={styles.heading}>
          Step {currentStep} of {step.totalSteps}
        </h1>
      </header>

      <section className={styles.instruction}>
        <p>{step.instruction}</p>
      </section>

      {step.timerMinutes != null && step.timerMinutes > 0 ? (
        <StepTimer key={`${currentStep}-${step.timerMinutes}`} minutes={step.timerMinutes} />
      ) : null}

      <nav className={styles.controls} aria-label="Step navigation">
        <Button variant="secondary" onClick={prev} disabled={!canPrev}>
          Previous
        </Button>
        <Button variant="primary" onClick={next} disabled={!canNext}>
          Next
        </Button>
      </nav>
    </article>
  );
}

export default CookModePage;
