import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";
import path from "path";

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  test: {
    globals: true,
    environment: "jsdom",
    setupFiles: ["./src/test/setup.integration.ts"],
    include: ["src/**/*.integration.test.{ts,tsx}"],
    env: {
      VITE_API_URL: "http://localhost",
    },
  },
});
