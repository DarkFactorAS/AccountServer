import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

// Dev server port and API target can be overridden:
// ACCOUNTWEB_DEV_PORT (default 5174) and ACCOUNTWEB_API_URL (default http://localhost:5000).
const devPort = Number(process.env.ACCOUNTWEB_DEV_PORT) || 5174;
const apiUrl = process.env.ACCOUNTWEB_API_URL || "http://localhost:5000";

export default defineConfig({
  plugins: [react(), tailwindcss()],
  build: { outDir: "dist", emptyOutDir: true },
  server: { port: devPort, strictPort: true, proxy: { "/api": apiUrl } },
});
