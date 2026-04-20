import { Button } from '../components/ui';
import styles from './FavoritesPage.module.css';

export function FavoritesPage() {
  return (
    <div className={styles.wrapper}>
      <h1>Favorites</h1>
      <p className={styles.note}>
        Favorites feature coming soon — this will be implemented in Challenge 02.
      </p>
      <Button variant="ghost" disabled>
        Add Favorite
      </Button>
    </div>
  );
}

export default FavoritesPage;
