import type { RefObject } from "react";
import { useHotkeys } from "react-hotkeys-hook";

export function useFormShortcuts({
  formRef,
}: {
  formRef: RefObject<HTMLFormElement | null>;
}) {
  useHotkeys(
    "mod+enter",
    () => {
      formRef.current?.requestSubmit();
    },
    { enableOnFormTags: true, preventDefault: true },
  );
}
