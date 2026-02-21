import { defineConfig } from "vite";
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
  server: {
    proxy: {
      "/api": {
        target: process.env.services__api__https__0 ?? process.env.services__api__http__0 ?? "https://localhost:5001",
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
