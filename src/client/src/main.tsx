import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router";
import {
  MutationCache,
  QueryCache,
  QueryClient,
  QueryClientProvider,
} from "@tanstack/react-query";
import { showApiError, showNetworkError } from "@/lib/toast";
import { TooltipProvider } from "@/components/ui/tooltip";
import { AuthProvider } from "@/contexts/AuthContext";
import "./index.css";
import App from "./App.tsx";

function handleGlobalError(error: unknown) {
  if (
    error &&
    typeof error === "object" &&
    "status" in error &&
    typeof (error as Record<string, unknown>).status === "number"
  ) {
    showApiError((error as Record<string, unknown>).status as number);
    return;
  }

  if (error instanceof TypeError && error.message === "Failed to fetch") {
    showNetworkError();
    return;
  }
}

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000,
      retry: 1,
    },
  },
  queryCache: new QueryCache({
    onError: handleGlobalError,
  }),
  mutationCache: new MutationCache({
    onError: handleGlobalError,
  }),
});

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthProvider>
          <TooltipProvider>
            <App />
          </TooltipProvider>
        </AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  </StrictMode>,
);
