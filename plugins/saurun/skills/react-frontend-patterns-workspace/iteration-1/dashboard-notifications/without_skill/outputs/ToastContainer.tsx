// ============================================================
// ToastContainer.tsx — Renders toast notifications
// ============================================================

import React from "react";
import { useToastStore } from "./toast-store";
import type { ToastVariant } from "./types";

const variantStyles: Record<ToastVariant, string> = {
  success: "bg-green-600 text-white",
  error: "bg-red-600 text-white",
  warning: "bg-yellow-500 text-black",
  info: "bg-blue-600 text-white",
};

export function ToastContainer() {
  const toasts = useToastStore((s) => s.toasts);
  const removeToast = useToastStore((s) => s.removeToast);

  if (toasts.length === 0) return null;

  return (
    <div className="fixed bottom-4 right-4 z-50 flex flex-col gap-2 max-w-sm">
      {toasts.map((t) => (
        <div
          key={t.id}
          className={`${variantStyles[t.variant]} rounded-lg px-4 py-3 shadow-lg flex items-center justify-between gap-3 animate-in slide-in-from-right`}
          role="alert"
        >
          <span className="text-sm font-medium">{t.message}</span>
          <button
            onClick={() => removeToast(t.id)}
            className="shrink-0 opacity-70 hover:opacity-100 transition-opacity"
            aria-label="Dismiss"
          >
            &times;
          </button>
        </div>
      ))}
    </div>
  );
}
