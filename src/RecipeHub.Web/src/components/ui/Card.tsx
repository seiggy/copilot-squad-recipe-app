import type { KeyboardEvent, ReactNode } from 'react';
import styles from './Card.module.css';

export type CardProps = {
  title?: ReactNode;
  footer?: ReactNode;
  onClick?: () => void;
  children: ReactNode;
  className?: string;
};

export function Card({ title, footer, onClick, children, className }: CardProps) {
  const clickable = typeof onClick === 'function';

  const handleKeyDown = (e: KeyboardEvent<HTMLDivElement>) => {
    if (!clickable) return;
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      onClick?.();
    }
  };

  const classes = [styles.card, clickable ? styles.clickable : '', className]
    .filter(Boolean)
    .join(' ');

  return (
    <div
      className={classes}
      onClick={clickable ? onClick : undefined}
      onKeyDown={clickable ? handleKeyDown : undefined}
      role={clickable ? 'button' : undefined}
      tabIndex={clickable ? 0 : undefined}
    >
      {title ? <div className={styles.title}>{title}</div> : null}
      <div className={styles.body}>{children}</div>
      {footer ? <div className={styles.footer}>{footer}</div> : null}
    </div>
  );
}

export default Card;
