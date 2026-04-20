import './App.css';

function App() {
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? '(not set)';

  return (
    <main>
      <h1>RecipeHub</h1>
      <p className="debug">VITE_API_BASE_URL: {apiBaseUrl}</p>
    </main>
  );
}

export default App;
