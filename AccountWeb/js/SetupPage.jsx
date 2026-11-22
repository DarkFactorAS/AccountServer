import { useState } from "react";
import CreateUserPanel from "./CreateUserPanel.jsx";
import ThemeToggle from "./ThemeToggle.jsx";
import { APP_TITLE } from "./constants.js";

// First-run setup: shown instead of the login page while no admin users are configured
// (and the request comes from this machine, unless AllowRemoteSetup is on).
export default function SetupPage({ theme, onThemeChange, configFile }) {
  const [created, setCreated] = useState(null);
  const snippet = created ? `"AdminUsers": ["${created.username}"]` : "";

  return (
    <div className="flex min-h-screen flex-col bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100">
      <header className="flex flex-col items-center gap-2 bg-slate-900 px-6 py-4 shadow lg:flex-row lg:justify-between">
        <h1 className="text-2xl font-bold text-white">{APP_TITLE}</h1>
        <ThemeToggle theme={theme} onChange={onThemeChange} />
      </header>
      <main className="mx-auto w-full max-w-2xl space-y-6 px-4 py-10">
        <section className="rounded-lg border-2 border-amber-400 bg-amber-50 p-4 text-sm dark:border-amber-600 dark:bg-amber-950">
          <h2 className="mb-1 font-semibold">First-run setup</h2>
          <p>
            No admin users are configured yet. Create the first account here; it will become the admin once you add
            it to <code>AdminUsers</code>. Setup closes as soon as one account has been created.
          </p>
        </section>
        {created ? (
          <section role="status" className="space-y-3 rounded-lg bg-white p-4 shadow dark:bg-slate-800">
            <h2 className="text-lg font-semibold">Account "{created.username}" created</h2>
            <ol className="list-decimal space-y-2 pl-5 text-sm">
              <li>
                Add it to <code>{configFile}</code>:
                <pre className="mt-1 overflow-x-auto rounded bg-slate-100 p-2 dark:bg-slate-900">{snippet}</pre>
              </li>
              <li>Restart AccountWeb (and turn <code>AllowRemoteSetup</code> off again if you turned it on).</li>
              <li>Reload this page and sign in as {created.username}.</li>
            </ol>
          </section>
        ) : (
          <CreateUserPanel
            endpoint="/api/setup/first-account"
            title="Create the first account"
            description="This account will be the first AccountWeb admin."
            submitLabel="Create first account"
            onCreated={setCreated}
          />
        )}
      </main>
    </div>
  );
}
