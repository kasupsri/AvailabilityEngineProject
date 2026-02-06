import type { User, CreateUserRequest } from '@/types/user';
import UserForm from '@/components/UserForm';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';

interface EditUserDialogProps {
  user: User | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSave: (id: number, user: CreateUserRequest) => Promise<void>;
  isLoading?: boolean;
}

export default function EditUserDialog({
  user,
  open,
  onOpenChange,
  onSave,
  isLoading = false,
}: EditUserDialogProps) {
  const initialData: CreateUserRequest | undefined = user
    ? {
        name: user.name,
        age: user.age,
        city: user.city,
        state: user.state,
        pincode: user.pincode,
      }
    : undefined;

  const handleSubmit = async (userData: CreateUserRequest) => {
    if (user) {
      await onSave(user.id, userData);
      onOpenChange(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Edit User</DialogTitle>
          <DialogDescription>
            Update the user information. All fields are required.
          </DialogDescription>
        </DialogHeader>
        <div className="py-4">
          {user && (
            <UserForm
              onSubmit={handleSubmit}
              isLoading={isLoading}
              initialData={initialData}
              submitLabel="Update User"
            />
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
