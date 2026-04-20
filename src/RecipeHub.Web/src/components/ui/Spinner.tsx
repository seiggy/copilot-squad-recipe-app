import styles from './Spinner.module.css';

export type SpinnerSize = 'sm' | 'md' | 'lg';

export type SpinnerProps = {
  size?: SpinnerSize;
  label?: string;
  className?: string;
};

export function Spinner({ size = 'md', label = 'Loading…', className }: SpinnerProps) {
  const classes = [styles.spinner, styles[size], className].filter(Boolean).join(' ');

  return (
    <span role="status" className={styles.wrapper}>
      <span className={classes} aria-hidden="true" />
      <span className={styles.srOnly}>{label}</span>
    </span>
  );
}

export default Spinner;
