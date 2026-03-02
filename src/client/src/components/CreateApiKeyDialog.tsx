import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import client from "@/lib/api-client";
import { showError } from "@/lib/toast";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Spinner } from "@/components/ui/spinner";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";

const createKeySchema = z.object({
  name: z.string().min(1, "Name is required"),
  expiresAt: z.string().optional(),
});

type CreateKeyFormValues = z.infer<typeof createKeySchema>;

interface CreateApiKeyDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onKeyCreated: (rawKey: string) => void;
}

export function CreateApiKeyDialog({
  open,
  onOpenChange,
  onKeyCreated,
}: CreateApiKeyDialogProps) {
  const queryClient = useQueryClient();

  const createMutation = useMutation({
    mutationFn: async (values: CreateKeyFormValues) => {
      const { data, error } = await client.POST("/api/apikeys", {
        body: {
          name: values.name,
          expiresAt: values.expiresAt || undefined,
        },
      });
      if (error) throw error;
      return data;
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ["apiKeys"] });
      if (data) {
        onKeyCreated(data.rawKey);
      }
      onOpenChange(false);
    },
    onError: () => {
      showError("Failed to create API key.");
    },
  });

  const form = useForm<CreateKeyFormValues>({
    resolver: zodResolver(createKeySchema),
    defaultValues: { name: "", expiresAt: "" },
  });

  function handleOpenChange(nextOpen: boolean) {
    if (nextOpen) {
      form.reset();
    }
    onOpenChange(nextOpen);
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Create API Key</DialogTitle>
          <DialogDescription>
            Generate a new API key for programmatic access.
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form
            onSubmit={form.handleSubmit((v) => createMutation.mutate(v))}
            className="space-y-4"
          >
            <FormField
              control={form.control}
              name="name"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Name</FormLabel>
                  <FormControl>
                    <Input
                      placeholder="e.g. Paperless Integration"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="expiresAt"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Expiration Date (optional)</FormLabel>
                  <FormControl>
                    <Input type="date" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <Button
              type="submit"
              className="w-full"
              disabled={createMutation.isPending}
            >
              {createMutation.isPending && <Spinner size="sm" />}
              {createMutation.isPending ? "Creating..." : "Create Key"}
            </Button>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
