import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  Form,
  FormField,
  FormItem,
  FormLabel,
  FormControl,
  FormMessage,
} from "./form";

// ---------------------------------------------------------------------------
// Test helpers
// ---------------------------------------------------------------------------

const schema = z.object({
  name: z.string().min(1, "Name is required"),
});

type TestValues = z.infer<typeof schema>;

function TestForm({
  onSubmit = () => {},
  defaultValues = { name: "" },
}: {
  onSubmit?: (values: TestValues) => void;
  defaultValues?: Partial<TestValues>;
}) {
  const form = useForm<TestValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: "", ...defaultValues },
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)}>
        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Name</FormLabel>
              <FormControl>
                <input {...field} aria-label="Name" />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <button type="submit">Submit</button>
      </form>
    </Form>
  );
}

// ---------------------------------------------------------------------------
// FormMessage visibility tests (regression for RECEIPTS-686)
// ---------------------------------------------------------------------------

describe("FormMessage", () => {
  it("renders visible validation error text when the field fails validation", async () => {
    const user = userEvent.setup();
    render(<TestForm />);

    // Submit without filling the required field to trigger the error
    await user.click(screen.getByRole("button", { name: /submit/i }));

    await waitFor(() => {
      expect(screen.getByText("Name is required")).toBeInTheDocument();
    });

    const message = screen.getByText("Name is required");

    // Must NOT carry sr-only (which makes it invisible to sighted users)
    expect(message.className).not.toContain("sr-only");
  });

  it("announces the error to screen readers via role=alert", async () => {
    const user = userEvent.setup();
    render(<TestForm />);

    await user.click(screen.getByRole("button", { name: /submit/i }));

    await waitFor(() => {
      expect(screen.getByRole("alert")).toBeInTheDocument();
    });

    expect(screen.getByRole("alert")).toHaveTextContent("Name is required");
  });

  it("remains associated with the input via aria-describedby for screen readers", async () => {
    const user = userEvent.setup();
    render(<TestForm />);

    await user.click(screen.getByRole("button", { name: /submit/i }));

    await waitFor(() => {
      expect(screen.getByText("Name is required")).toBeInTheDocument();
    });

    const input = screen.getByLabelText("Name");
    const message = screen.getByRole("alert");

    // The input's aria-describedby should include the message element's id
    const describedBy = input.getAttribute("aria-describedby") ?? "";
    expect(describedBy).toContain(message.id);
  });

  it("renders nothing when there is no error and no children", async () => {
    const onSubmit = vi.fn();
    render(<TestForm onSubmit={onSubmit} defaultValues={{ name: "Alice" }} />);

    // No alert should be present when the form is valid and untouched
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
  });

  it("clears the error message once the field becomes valid", async () => {
    const user = userEvent.setup();
    render(<TestForm />);

    // Trigger error
    await user.click(screen.getByRole("button", { name: /submit/i }));
    await waitFor(() => expect(screen.getByRole("alert")).toBeInTheDocument());

    // Fix the field
    await user.type(screen.getByLabelText("Name"), "Alice");
    await user.click(screen.getByRole("button", { name: /submit/i }));

    await waitFor(() => {
      expect(screen.queryByRole("alert")).not.toBeInTheDocument();
    });
  });
});
