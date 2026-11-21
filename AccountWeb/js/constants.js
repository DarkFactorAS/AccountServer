export const APP_TITLE = "DarkFactor - Account administration";

// Mirrors the AccountServer rules (AccountProvider.VerifyPassword / VerifyNickname) so mistakes
// show before submitting. The server stays the authority.
export const PASSWORD_MIN = 8;
export const PASSWORD_MAX = 20;
export const PASSWORD_PATTERN = /^[a-zA-Z0-9%*()]+$/;
export const NICKNAME_PATTERN = /^[a-zA-Z0-9]+$/;

export const INPUT_CLASS = "w-full rounded border border-slate-300 dark:border-slate-600 dark:bg-slate-900 p-2 text-sm";
export const PASSWORD_HINT = `${PASSWORD_MIN}-${PASSWORD_MAX} characters: A-Z, 0-9 and %*()`;

// Returns the first problem with a new password (and its confirmation), or null.
export function passwordProblem(password, confirm) {
  if (password.length < PASSWORD_MIN || password.length > PASSWORD_MAX)
    return `Password must be between ${PASSWORD_MIN} and ${PASSWORD_MAX} characters long.`;
  if (!PASSWORD_PATTERN.test(password)) return "Password can only contain A-Z, 0-9 and %*().";
  if (password !== confirm) return "Passwords do not match.";
  return null;
}
