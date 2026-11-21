import { useState } from "react";
import { apiFetch, errorMessage } from "./auth.jsx";
import { Field } from "./CreateUserPanel.jsx";
import { NICKNAME_PATTERN, PASSWORD_HINT, passwordProblem } from "./constants.js";

function formatDate(value) {
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "-" : date.toLocaleString();
}

function Message({ error, success }) {
  if (error) return <p role="alert" className="text-sm text-red-600 dark:text-red-400">{error}</p>;
  if (success) return <p role="status" className="text-sm text-emerald-700 dark:text-emerald-400">{success}</p>;
  return null;
}

// Edit details and set a new password for one account. onSaved(user) gets the updated account.
export default function EditUserPanel({ user, onSaved, onClose }) {
  const [details, setDetails] = useState({ nickname: user.nickname, email: user.email });
  const [detailsState, setDetailsState] = useState({ busy: false, error: null, success: null });
  const [password, setPassword] = useState({ password: "", confirm: "" });
  const [passwordState, setPasswordState] = useState({ busy: false, error: null, success: null });
  const id = `user-${user.id}`;

  async function saveDetails(event) {
    event.preventDefault();
    const nickname = details.nickname.trim();
    if (!nickname) return setDetailsState({ busy: false, error: "Nickname is required.", success: null });
    if (!NICKNAME_PATTERN.test(nickname))
      return setDetailsState({ busy: false, error: "Nickname can only contain A-Z and 0-9.", success: null });
    if (nickname === user.username)
      return setDetailsState({ busy: false, error: "Nickname and username cannot be equal.", success: null });

    setDetailsState({ busy: true, error: null, success: null });
    try {
      const response = await apiFetch(`/api/accounts/${user.id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ nickname, email: details.email.trim() }),
      });
      if (!response.ok) {
        setDetailsState({ busy: false, error: await errorMessage(response, `Could not save (${response.status}).`), success: null });
        return;
      }
      const updated = await response.json();
      setDetailsState({ busy: false, error: null, success: "Saved." });
      onSaved?.(updated);
    } catch {
      setDetailsState({ busy: false, error: "Unable to reach AccountWeb.", success: null });
    }
  }

  async function savePassword(event) {
    event.preventDefault();
    const problem = passwordProblem(password.password, password.confirm);
    if (problem) return setPasswordState({ busy: false, error: problem, success: null });

    setPasswordState({ busy: true, error: null, success: null });
    try {
      const response = await apiFetch(`/api/accounts/${user.id}/password`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ password: password.password }),
      });
      setPassword({ password: "", confirm: "" });
      if (!response.ok) {
        setPasswordState({ busy: false, error: await errorMessage(response, `Could not set the password (${response.status}).`), success: null });
        return;
      }
      setPasswordState({ busy: false, error: null, success: "Password changed. Saved logins on other devices were signed out." });
    } catch {
      setPasswordState({ busy: false, error: "Unable to reach AccountWeb.", success: null });
    }
  }

  return (
    <div className="mt-3 space-y-4 border-t border-slate-200 pt-3 dark:border-slate-700">
      <dl className="grid grid-cols-2 gap-x-4 gap-y-1 text-xs text-slate-500 dark:text-slate-400">
        <dt>Login type</dt><dd>{user.loginType}</dd>
        <dt>Created</dt><dd>{formatDate(user.created)}</dd>
        <dt>Last login</dt><dd>{formatDate(user.lastLogin)}</dd>
        <dt>Logins</dt><dd>{user.numLogins}</dd>
      </dl>

      <form onSubmit={saveDetails} noValidate className="space-y-3">
        <h3 className="text-sm font-semibold">Details</h3>
        <Field id={`${id}-nickname`} label="Nickname" maxLength={100} autoComplete="off"
          value={details.nickname} onChange={(e) => setDetails((d) => ({ ...d, nickname: e.target.value }))} />
        <Field id={`${id}-email`} label="Email" type="email" maxLength={100} autoComplete="off"
          value={details.email} onChange={(e) => setDetails((d) => ({ ...d, email: e.target.value }))} />
        <Message {...detailsState} />
        <div className="flex justify-end">
          <button type="submit" disabled={detailsState.busy}
            className="rounded bg-slate-900 px-3 py-1.5 text-sm font-semibold text-white disabled:opacity-50 dark:bg-sky-700">
            {detailsState.busy ? "Saving..." : "Save details"}
          </button>
        </div>
      </form>

      <form onSubmit={savePassword} noValidate className="space-y-3">
        <h3 className="text-sm font-semibold">Set new password</h3>
        <Field id={`${id}-password`} label="New password" type="password"
          autoComplete="new-password" hint={PASSWORD_HINT}
          value={password.password} onChange={(e) => setPassword((p) => ({ ...p, password: e.target.value }))} />
        <Field id={`${id}-confirm`} label="Confirm new password" type="password"
          autoComplete="new-password"
          value={password.confirm} onChange={(e) => setPassword((p) => ({ ...p, confirm: e.target.value }))} />
        <Message {...passwordState} />
        <div className="flex justify-end gap-2">
          <button type="button" onClick={onClose}
            className="rounded border border-slate-300 px-3 py-1.5 text-sm dark:border-slate-600">
            Close
          </button>
          <button type="submit" disabled={passwordState.busy}
            className="rounded bg-slate-900 px-3 py-1.5 text-sm font-semibold text-white disabled:opacity-50 dark:bg-sky-700">
            {passwordState.busy ? "Saving..." : "Set password"}
          </button>
        </div>
      </form>
    </div>
  );
}
