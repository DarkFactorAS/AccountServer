import { useEffect, useState } from "react";
import { AuthProvider, useAuth } from "./auth.jsx";
import LoginPage from "./LoginPage.jsx";
import SetupPage from "./SetupPage.jsx";
import CreateUserPanel from "./CreateUserPanel.jsx";
import UsersPanel from "./UsersPanel.jsx";
import ThemeToggle from "./ThemeToggle.jsx";
import useTheme from "./useTheme.js";
import { APP_TITLE } from "./constants.js";

function Dashboard({ theme, setTheme }) {
  const { username, logout } = useAuth();
  // Bumped after a user is created so the Users list reloads.
  const [usersVersion, setUsersVersion] = useState(0);
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
      <main className="mx-auto grid max-w-6xl grid-cols-1 items-start gap-6 p-6 lg:grid-cols-2">
        <CreateUserPanel onCreated={() => setUsersVersion((v) => v + 1)} />
        <UsersPanel refreshKey={usersVersion} />
      </main>
    </div>
  );
}

// Asks the server whether first-run setup is open; only checked while signed out.
// Returns null while loading, then { setupMode, configFile? }.
function useSetupStatus(enabled) {
  const [setup, setSetup] = useState(null);
  useEffect(() => {
    if (!enabled) return undefined;
    let cancelled = false;
    fetch("/api/setup/status", { credentials: "same-origin" })
      .then((response) => (response.ok ? response.json() : { setupMode: false }))
      .catch(() => ({ setupMode: false }))
      .then((body) => {
        if (!cancelled) setSetup(body);
      });
    return () => {
      cancelled = true;
    };
  }, [enabled]);
  return setup;
}

function AuthGate() {
  const { status } = useAuth();
  const { theme, setTheme } = useTheme();
  const setup = useSetupStatus(status === "anonymous");
  if (status === "loading" || (status === "anonymous" && setup === null)) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-slate-50 dark:bg-slate-950 text-slate-500 dark:text-slate-400">
        <p role="status">Loading...</p>
      </div>
    );
  }
  if (status !== "authenticated") {
    if (setup?.setupMode) {
      return <SetupPage theme={theme} onThemeChange={setTheme} configFile={setup.configFile} />;
    }
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
