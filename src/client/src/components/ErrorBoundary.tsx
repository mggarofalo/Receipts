import * as Sentry from "@sentry/react";
import type { ReactNode } from "react";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
} from "@/components/ui/card";

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

function ErrorFallback({
  error,
  resetError,
}: {
  error: Error;
  resetError: () => void;
}) {
  return (
    <main
      className="flex min-h-screen items-center justify-center p-4"
      role="alert"
    >
      <Card className="w-full max-w-md">
        <CardHeader>
          <h1 className="text-lg font-semibold leading-none tracking-tight">
            Something went wrong
          </h1>
          <CardDescription>
            An unexpected error occurred. Please try again.
          </CardDescription>
        </CardHeader>
        <CardContent className="flex flex-col gap-4">
          {error && (
            <pre className="rounded-md bg-muted p-3 text-sm text-muted-foreground overflow-auto">
              {error.message}
            </pre>
          )}
          <div className="flex gap-2">
            <Button onClick={resetError}>Try Again</Button>
            <Button
              variant="outline"
              onClick={() => window.location.assign("/")}
            >
              Go Home
            </Button>
          </div>
        </CardContent>
      </Card>
    </main>
  );
}

export function ErrorBoundary({ children, fallback }: Props) {
  return (
    <Sentry.ErrorBoundary
      fallback={
        fallback
          ? () => <>{fallback}</>
          : ({ error, resetError }) => (
              <ErrorFallback
                error={error instanceof Error ? error : new Error(String(error))}
                resetError={resetError}
              />
            )
      }
    >
      {children}
    </Sentry.ErrorBoundary>
  );
}
