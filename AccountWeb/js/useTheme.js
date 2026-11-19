import { useCallback, useEffect, useState } from "react";

export const THEME_STORAGE_KEY = "accountweb.theme";
export const THEMES = ["system", "light", "dark"];

function readStoredTheme() {
  try {
    const value = window.localStorage.getItem(THEME_STORAGE_KEY);
    return THEMES.includes(value) ? value : "system";
  } catch {
    return "system";
  }
}

function systemPrefersDark() {
  return typeof window.matchMedia === "function" && window.matchMedia("(prefers-color-scheme: dark)").matches;
}

// Holds the theme preference (system | light | dark), persists it in localStorage and
// toggles the "dark" class on <html>.
export default function useTheme() {
  const [theme, setThemeState] = useState(readStoredTheme);
  const [prefersDark, setPrefersDark] = useState(systemPrefersDark);

  useEffect(() => {
    if (typeof window.matchMedia !== "function") return undefined;
    const query = window.matchMedia("(prefers-color-scheme: dark)");
    const onChange = (event) => setPrefersDark(event.matches);
    query.addEventListener("change", onChange);
    return () => query.removeEventListener("change", onChange);
  }, []);

  const resolvedTheme = theme === "system" ? (prefersDark ? "dark" : "light") : theme;

  useEffect(() => {
    document.documentElement.classList.toggle("dark", resolvedTheme === "dark");
  }, [resolvedTheme]);

  const setTheme = useCallback((next) => {
    if (!THEMES.includes(next)) return;
    setThemeState(next);
    try {
      window.localStorage.setItem(THEME_STORAGE_KEY, next);
    } catch {
      // Storage may be blocked; the choice then only lasts for this page view.
    }
  }, []);

  return { theme, resolvedTheme, setTheme };
}
