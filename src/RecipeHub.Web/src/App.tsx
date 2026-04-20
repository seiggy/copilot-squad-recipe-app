import { NavLink, Route, Routes, useLocation } from 'react-router-dom';
import {
  CookModePage,
  FavoritesPage,
  HomePage,
  RecipeDetailPage,
  RecipeEditPage,
  RecipeListPage,
  SharedRecipePage,
} from './pages';
import styles from './App.module.css';
import './App.css';

function App() {
  const location = useLocation();
  const hideChrome = location.pathname.startsWith('/shared/');

  const navLinkClass = ({ isActive }: { isActive: boolean }) =>
    isActive ? styles.active : undefined;

  if (hideChrome) {
    return (
      <Routes>
        <Route path="/shared/:token" element={<SharedRecipePage />} />
      </Routes>
    );
  }

  return (
    <div className={styles.app}>
      <nav className={styles.nav}>
        <NavLink to="/" className={styles.brand}>
          RecipeHub
        </NavLink>
        <NavLink to="/" end className={navLinkClass}>
          Home
        </NavLink>
        <NavLink to="/recipes" className={navLinkClass}>
          Recipes
        </NavLink>
        <NavLink to="/favorites" className={navLinkClass}>
          Favorites
        </NavLink>
      </nav>
      <main>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/recipes" element={<RecipeListPage />} />
          <Route path="/recipes/new" element={<RecipeEditPage />} />
          <Route path="/recipes/:id" element={<RecipeDetailPage />} />
          <Route path="/recipes/:id/edit" element={<RecipeEditPage />} />
          <Route path="/recipes/:id/cook" element={<CookModePage />} />
          <Route path="/favorites" element={<FavoritesPage />} />
        </Routes>
      </main>
    </div>
  );
}

export default App;
