import { Badge, Spinner } from '../ui';
import { useTags } from '../../hooks';
import styles from './FilterPanel.module.css';

export interface FilterPanelProps {
  selectedTag: string | undefined;
  onTagChange: (tag: string | undefined) => void;
}

export function FilterPanel({ selectedTag, onTagChange }: FilterPanelProps) {
  const { data, isLoading, isError } = useTags();

  if (isLoading) {
    return <Spinner label="Loading tags…" size="sm" />;
  }

  if (isError || !data) {
    return <div className={styles.error}>Couldn't load tags.</div>;
  }

  if (data.length === 0) {
    return null;
  }

  return (
    <div className={styles.panel} role="group" aria-label="Filter by tag">
      {data.map((tag) => {
        const active = selectedTag === tag.name;
        return (
          <button
            key={tag.id}
            type="button"
            className={`${styles.chip} ${active ? styles.active : ''}`}
            onClick={() => onTagChange(active ? undefined : tag.name)}
            aria-pressed={active}
          >
            <Badge variant={active ? 'success' : 'info'}>{tag.name}</Badge>
          </button>
        );
      })}
    </div>
  );
}

export default FilterPanel;
