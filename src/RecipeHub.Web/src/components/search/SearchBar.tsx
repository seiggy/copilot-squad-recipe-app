import { useEffect, useState } from 'react';
import styles from './SearchBar.module.css';

export interface SearchBarProps {
  value: string;
  onChange: (next: string) => void;
  placeholder?: string;
  debounceMs?: number;
}

export function SearchBar({
  value,
  onChange,
  placeholder = 'Search recipes…',
  debounceMs = 300,
}: SearchBarProps) {
  const [local, setLocal] = useState(value);

  useEffect(() => {
    setLocal(value);
  }, [value]);

  useEffect(() => {
    if (local === value) return;
    const handle = window.setTimeout(() => {
      onChange(local);
    }, debounceMs);
    return () => window.clearTimeout(handle);
  }, [local, value, onChange, debounceMs]);

  return (
    <div className={styles.wrapper}>
      <input
        type="search"
        className={styles.input}
        placeholder={placeholder}
        value={local}
        onChange={(e) => setLocal(e.target.value)}
        aria-label="Search recipes"
      />
      {local.length > 0 ? (
        <button
          type="button"
          className={styles.clear}
          onClick={() => {
            setLocal('');
            onChange('');
          }}
          aria-label="Clear search"
        >
          ×
        </button>
      ) : null}
    </div>
  );
}

export default SearchBar;
