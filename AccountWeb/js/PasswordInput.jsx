import { useState } from "react";
import { INPUT_CLASS } from "./constants.js";

function EyeIcon({ open }) {
  return (
    <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" strokeWidth="2"
      strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
      <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12z" />
      <circle cx="12" cy="12" r="3" />
      {!open && <path d="M3 3l18 18" />}
    </svg>
  );
}

// Password field with an eye button that shows/hides what was typed. Accepts the same props as <input>.
export default function PasswordInput({ id, className = INPUT_CLASS, ...props }) {
  const [visible, setVisible] = useState(false);
  return (
    <div className="relative">
      <input id={id} {...props} type={visible ? "text" : "password"} className={`${className} pr-10`} />
      <button
        type="button"
        onClick={() => setVisible((v) => !v)}
        aria-label={visible ? "Hide password" : "Show password"}
        aria-pressed={visible}
        aria-controls={id}
        title={visible ? "Hide password" : "Show password"}
        className="absolute inset-y-0 right-0 flex items-center rounded-r px-2 text-slate-500 hover:text-slate-800 focus:outline-none focus:ring-2 focus:ring-sky-400 dark:text-slate-400 dark:hover:text-slate-100"
      >
        <EyeIcon open={!visible} />
      </button>
    </div>
  );
}
