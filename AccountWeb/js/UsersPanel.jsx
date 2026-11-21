import { useCallback, useEffect, useRef, useState } from "react";
import { apiFetch, errorMessage } from "./auth.jsx";
import { INPUT_CLASS } from "./constants.js";
import EditUserPanel from "./EditUserPanel.jsx";

// Search accounts and open one to edit it. refreshKey changes when a user was created elsewhere.
export default function UsersPanel({ refreshKey }) {
  const [query, setQuery] = useState("");
  const [state, setState] = useState({ users: null, loading: true, error: null });
  const [openId, setOpenId] = useState(null);
  const requestId = useRef(0);

  const load = useCallback(async (search) => {
    const current = ++requestId.current;
    setState((s) => ({ ...s, loading: true, error: null }));
    try {
      const response = await apiFetch(`/api/accounts?query=${encodeURIComponent(search)}`);
      if (current !== requestId.current) return; // a newer search is running
      if (!response.ok) {
        setState({ users: null, loading: false, error: await errorMessage(response, `Could not load users (${response.status}).`) });
        return;
      }
      setState({ users: await response.json(), loading: false, error: null });
    } catch {
      if (current === requestId.current) setState({ users: null, loading: false, error: "Unable to reach AccountWeb." });
    }
  }, []);

  // Debounced search as you type; also reloads when a user is created.
  useEffect(() => {
    const timer = setTimeout(() => load(query.trim()), 300);
    return () => clearTimeout(timer);
  }, [query, refreshKey, load]);

  const replaceUser = (updated) =>
    setState((s) => ({ ...s, users: s.users?.map((u) => (u.id === updated.id ? updated : u)) ?? null }));

  return (
    <section className="rounded-lg bg-white p-4 shadow dark:bg-slate-800">
      <div className="mb-3 flex items-center justify-between gap-2">
        <h2 className="text-lg font-semibold">Users</h2>
        <button type="button" onClick={() => load(query.trim())}
          className="rounded border border-slate-300 px-2 py-1 text-sm dark:border-slate-600">
          Refresh
        </button>
      </div>
      <label htmlFor="user-search" className="sr-only">Search users</label>
      <input
        id="user-search"
        type="search"
        placeholder="Search username, nickname or email"
        maxLength={100}
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        className={`${INPUT_CLASS} mb-3`}
      />
      {state.loading && !state.users && <p role="status" className="text-slate-500 dark:text-slate-400">Loading...</p>}
      {state.error && <p role="alert" className="text-red-600 dark:text-red-400">{state.error}</p>}
      {state.users && state.users.length === 0 && (
        <p className="text-slate-500 dark:text-slate-400">{query.trim() ? "No users match." : "No users yet."}</p>
      )}
      {state.users && state.users.length > 0 && (
        <>
          <ul className="space-y-3">
            {state.users.map((user) => (
              <li key={user.id} className="rounded border-2 border-slate-200 p-3 dark:border-slate-700">
                <button
                  type="button"
                  aria-expanded={openId === user.id}
                  onClick={() => setOpenId(openId === user.id ? null : user.id)}
                  className="block w-full text-left"
                >
                  <div className="flex items-center justify-between gap-2">
                    <span className="font-semibold">{user.username}</span>
                    <span className="flex gap-1">
                      {user.loginType === "GameCenter" && (
                        <span className="rounded bg-yellow-100 px-2 py-0.5 text-xs text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200">
                          GameCenter
                        </span>
                      )}
                      <span className="rounded bg-slate-100 px-2 py-0.5 text-xs dark:bg-slate-700">id {user.id}</span>
                    </span>
                  </div>
                  <p className="mt-1 text-sm">{user.nickname}</p>
                  {user.email && <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">{user.email}</p>}
                </button>
                {openId === user.id && (
                  <EditUserPanel user={user} onSaved={replaceUser} onClose={() => setOpenId(null)} />
                )}
              </li>
            ))}
          </ul>
          {state.users.length >= 50 && (
            <p className="mt-3 text-xs text-slate-500 dark:text-slate-400">Showing the newest 50 matches. Refine the search to find others.</p>
          )}
        </>
      )}
    </section>
  );
}
