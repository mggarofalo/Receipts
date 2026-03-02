import { useDeleteUser } from "@/hooks/useUsers";
import { Spinner } from "@/components/ui/spinner";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";

interface DeactivateUserDialogProps {
  user: { id: string; email: string } | null;
  onClose: () => void;
}

export function DeactivateUserDialog({
  user,
  onClose,
}: DeactivateUserDialogProps) {
  const deleteUser = useDeleteUser();

  return (
    <AlertDialog open={!!user} onOpenChange={() => onClose()}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Deactivate User</AlertDialogTitle>
          <AlertDialogDescription>
            Are you sure you want to deactivate{" "}
            <span className="font-semibold">{user?.email}</span>? They will no
            longer be able to log in.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Cancel</AlertDialogCancel>
          <AlertDialogAction
            variant="destructive"
            onClick={async () => {
              if (user) {
                await deleteUser.mutateAsync(user.id);
                onClose();
              }
            }}
          >
            {deleteUser.isPending && <Spinner size="sm" />}
            Deactivate
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
