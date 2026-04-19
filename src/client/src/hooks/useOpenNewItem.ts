import { useEffect } from "react";
import { useLocation, useNavigate } from "react-router";

// Navigation-state shape used by the command palette's "New X" commands to ask
// a target list page to open its create dialog on mount. Using state instead
// of a post-navigate setTimeout avoids a race: the listener doesn't need to
// already be mounted when the command fires.
interface OpenNewItemState {
  openNew?: boolean;
}

/**
 * Wire a list page's create dialog to both the Shift+N global shortcut and
 * the command palette's cross-page "New X" action.
 *
 * - Subscribes to the `shortcut:new-item` window event (Shift+N, same-page
 *   palette actions).
 * - Reads `location.state.openNew` on mount/navigation; if true, calls `open`
 *   and strips the flag so browser back/forward won't re-trigger.
 *
 * Pass a stable callback (e.g. a `useCallback` that closes over a
 * `useState` setter) so the subscription effect doesn't resubscribe.
 */
export function useOpenNewItem(open: () => void) {
  const location = useLocation();
  const navigate = useNavigate();

  useEffect(() => {
    window.addEventListener("shortcut:new-item", open);
    return () => window.removeEventListener("shortcut:new-item", open);
  }, [open]);

  useEffect(() => {
    const state = location.state as OpenNewItemState | null;
    if (state?.openNew) {
      open();
      navigate(location.pathname + location.search, {
        replace: true,
        state: null,
      });
    }
  }, [location, navigate, open]);
}
