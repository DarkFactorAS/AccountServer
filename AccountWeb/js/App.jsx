import { useState } from "react";
import { AuthProvider, useAuth } from "./auth.jsx";
import LoginPage from "./LoginPage.jsx";
import CreateUserPanel from "./CreateUserPanel.jsx";
import ThemeToggle from "./ThemeToggle.jsx";
import useTheme from "./useTheme.js";
import { APP_TITLE } from "./constants.js";

// Users created from this browser tab (not persisted; a quick confirmation trail).
function RecentUsersPanel({ users }) {
  return (
    <section className="rounded-lg bg-white dark:bg-slate-800 p-4 shadow">
      <h2 className="mb-3 text-lg font-semibold">Created this session</h2>
      {users.length === 0 ? (
        <p className="text-slate-500 dark:text-slate-400">No users created yet.</p>
      ) : (
        <ul className="space-y-3">
          {users.map((user) => (
            <li key={user.id} className="rounded border-2 border-slate-200 dark:border-slate-700 p-3">
              <div className="flex items-center justify-between gap-2">
                <span className="font-semibold">{user.username}</span>
                <span className="rounded bg-slate-100 dark:bg-slate-700 px-2 py-0.5 text-xs">id {user.id}</span>
              </div>
              <p className="mt-1 text-sm">{user.nickname}</p>
              <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">{user.createdAt.toLocaleTimeString()}</p>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}

function Dashboard({ theme, setTheme }) {
  const { username, logout } = useAuth();
  const [created, setCreated] = useState([]);
  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100">
      <header className="bg-slate-900 px-6 py-4 shadow lg:grid lg:grid-cols-[1fr_auto_1fr] lg:items-center lg:gap-4">
        <div className="hidden lg:block" />
        <h1 className="text-2xl font-bold text-white text-center">{APP_TITLE}</h1>
        <div className="mt-2 flex flex-wrap items-center justify-center gap-3 lg:mt-0 lg:justify-end">
          <ThemeToggle theme={theme} onChange={setTheme} />
          {username && (
            <span className="text-sm text-slate-300">
              Signed in as <span className="font-semibold text-white">{username}</span>
            </span>
          )}
          <button
            type="button"
            onClick={logout}
            className="rounded border border-slate-500 px-2 py-1 text-sm text-white hover:bg-slate-700 focus:outline-none focus:ring-2 focus:ring-sky-400"
          >
            Log out
          </button>
        </div>
      </header>
      <main className="mx-auto grid max-w-6xl grid-cols-1 gap-6 p-6 lg:grid-cols-[2fr_1fr]">
        <CreateUserPanel onCreated={(user) => setCreated((list) => [user, ...list])} />
        <RecentUsersPanel users={created} />
      </main>
    </div>
  );
}

function AuthGate() {
  const { status } = useAuth();
  const { theme, setTheme } = useTheme();
  if (status === "loading") {
    return (
      <div className="flex min-h-screen items-center justify-center bg-slate-50 dark:bg-slate-950 text-slate-500 dark:text-slate-400">
        <p role="status">Loading...</p>
      </div>
    );
  }
  if (status !== "authenticated") {
    return <LoginPage theme={theme} onThemeChange={setTheme} />;
  }
  return <Dashboard theme={theme} setTheme={setTheme} />;
}

export default function App() {
  return (
    <AuthProvider>
      <AuthGate />
    </AuthProvider>
  );
}
