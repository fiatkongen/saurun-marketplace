import { X } from "lucide-react";
import type { Toast } from "./use-toast";

const variantStyles: Record<Toast["variant"], string> = {
  success: "bg-emerald-600 text-white dark:bg-emerald-500",
  error: "bg-red-600 text-white dark:bg-red-500",
  info: "bg-slate-800 text-white dark:bg-slate-200 dark:text-slate-900",
};

interface ToastContainerProps {
  toasts: Toast[];
  onDismiss: (id: string) => void;
}

export function ToastContainer({ toasts, onDismiss }: ToastContainerProps) {
  if (toasts.length === 0) return null;

  return (
    <div className="fixed bottom-6 right-6 z-50 flex flex-col gap-2">
      {toasts.map((toast) => (
        <div
          key={toast.id}
          className={`flex items-center gap-3 rounded-lg px-4 py-3 shadow-lg text-sm font-medium animate-in slide-in-from-bottom-4 fade-in duration-300 ${variantStyles[toast.variant]}`}
        >
          <span>{toast.message}</span>
          <button
            onClick={() => onDismiss(toast.id)}
            className="ml-2 rounded-md p-0.5 opacity-70 hover:opacity-100 transition-opacity"
            aria-label="Dismiss"
          >
            <X className="h-4 w-4" />
          </button>
        </div>
      ))}
    </div>
  );
}
