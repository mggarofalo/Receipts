import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import path from "path";

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  test: {
    globals: true,
    environment: "jsdom",
    setupFiles: ["./src/test/setup.ts"],
    include: ["src/**/*.test.{ts,tsx}"],
    coverage: {
      provider: "v8",
      reporter: ["text", "cobertura", "html"],
      reportsDirectory: "./coverage",
      include: ["src/**/*.{ts,tsx}"],
      exclude: [
        "src/generated/**",
        "**/*.d.ts",
        "**/*.test.{ts,tsx}",
        "src/test/**",
        "src/main.tsx",
        "src/components/ui/!(combobox|currency-input).{ts,tsx}",
        "src/lib/api-types.ts",
      ],
      thresholds: {
        statements: 75,
        branches: 65,
        functions: 70,
        lines: 78,
      },
    },
  },
  server: {
    proxy: {
      "/api": {
        target: process.env.services__api__https__0 ?? process.env.services__api__http__0 ?? "https://localhost:5001",
        changeOrigin: true,
        secure: false,
      },
      "/hubs": {
        target: process.env.services__api__https__0 ?? process.env.services__api__http__0 ?? "https://localhost:5001",
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
  },
});
