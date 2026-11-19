import { useState } from "react";
import { apiFetch, errorMessage } from "./auth.jsx";
import { INPUT_CLASS, NICKNAME_PATTERN, PASSWORD_HINT, PASSWORD_MAX, passwordProblem } from "./constants.js";

const EMPTY = { username: "", nickname: "", email: "", password: "", confirm: "" };

// Returns the first client-side problem with the form, or null.
export function validate(form) {
  const username = form.username.trim();
  const nickname = form.nickname.trim();
  if (!username || !nickname || !form.password) return "Username, nickname and password are required.";
  if (!/^[a-zA-Z0-9]/.test(username)) return "Username must start with a letter or digit.";
  if (!NICKNAME_PATTERN.test(nickname)) return "Nickname can only contain A-Z and 0-9.";
  if (nickname === username) return "Nickname and username cannot be equal.";
  return passwordProblem(form.password, form.confirm);
}

export function Field({ id, label, hint, ...props }) {
  return (
    <div>
      <label htmlFor={id} className="mb-1 block text-sm font-semibold">{label}</label>
      <input id={id} className={INPUT_CLASS} aria-describedby={hint ? `${id}-hint` : undefined} {...props} />
      {hint && <p id={`${id}-hint`} className="mt-1 text-xs text-slate-500 dark:text-slate-400">{hint}</p>}
    </div>
  );
}

export default function CreateUserPanel({
  onCreated,
  endpoint = "/api/accounts",
  title = "Create user",
  description = "Adds a new account to the account database.",
  submitLabel = "Create user",
}) {
  const [form, setForm] = useState(EMPTY);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);
  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));

  async function handleSubmit(event) {
    event.preventDefault();
    if (busy) return;
    const problem = validate(form);
    setSuccess(null);
    if (problem) {
      setError(problem);
      return;
    }
    setBusy(true);
    setError(null);
    try {
      const response = await apiFetch(endpoint, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          username: form.username.trim(),
          nickname: form.nickname.trim(),
          email: form.email.trim(),
          password: form.password,
        }),
      });
      if (!response.ok) {
        setError(await errorMessage(response, `Could not create the user (${response.status}).`));
        setForm((f) => ({ ...f, password: "", confirm: "" }));
        return;
      }
      const created = await response.json();
      setSuccess(`User "${created.username}" created (id ${created.id}).`);
      setForm(EMPTY);
      onCreated?.({ ...created, createdAt: new Date() });
    } catch {
      setError("Unable to reach AccountWeb.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <section className="rounded-lg bg-white dark:bg-slate-800 p-4 shadow">
      <div className="mb-3">
        <h2 className="text-lg font-semibold">{title}</h2>
        <p className="text-sm text-slate-500 dark:text-slate-400">{description}</p>
      </div>
      <form onSubmit={handleSubmit} className="space-y-4" noValidate>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Field id="new-username" label="Username" required maxLength={100} autoComplete="off"
            value={form.username} onChange={set("username")} />
          <Field id="new-nickname" label="Nickname" required maxLength={100} autoComplete="off"
            hint="Shown to other players. A-Z and 0-9, not the same as the username."
            value={form.nickname} onChange={set("nickname")} />
          <Field id="new-password" label="Password" type="password" required maxLength={PASSWORD_MAX}
            autoComplete="new-password" hint={PASSWORD_HINT}
            value={form.password} onChange={set("password")} />
          <Field id="new-confirm" label="Confirm password" type="password" required maxLength={PASSWORD_MAX}
            autoComplete="new-password" value={form.confirm} onChange={set("confirm")} />
          <div className="sm:col-span-2">
            <Field id="new-email" label="Email (optional)" type="email" maxLength={100} autoComplete="off"
              hint="Used for password reset." value={form.email} onChange={set("email")} />
          </div>
        </div>
        {error && <p role="alert" className="text-sm text-red-600 dark:text-red-400">{error}</p>}
        {success && <p role="status" className="text-sm text-emerald-700 dark:text-emerald-400">{success}</p>}
        <div className="flex justify-end gap-2">
          <button
            type="button"
            onClick={() => { setForm(EMPTY); setError(null); setSuccess(null); }}
            className="rounded border border-slate-300 dark:border-slate-600 px-3 py-2 text-sm"
          >
            Clear
          </button>
          <button
            type="submit"
            disabled={busy}
            className="rounded bg-slate-900 dark:bg-sky-700 px-4 py-2 text-sm font-semibold text-white disabled:opacity-50"
          >
            {busy ? "Creating..." : submitLabel}
          </button>
        </div>
      </form>
    </section>
  );
}
