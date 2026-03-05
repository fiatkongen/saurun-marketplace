// ============================================================
// toast-store.ts — Global toast notification store (Zustand)
// ============================================================

import { create } from "zustand";
import type { Toast, ToastVariant } from "./types";

interface ToastState {
  toasts: Toast[];
  addToast: (message: string, variant?: ToastVariant, duration?: number) => void;
  removeToast: (id: string) => void;
}

let nextId = 0;

export const useToastStore = create<ToastState>((set) => ({
  toasts: [],

  addToast: (message, variant = "info", duration = 5000) => {
    const id = String(++nextId);
    const toast: Toast = { id, message, variant, duration };

    set((state) => ({ toasts: [...state.toasts, toast] }));

    if (duration > 0) {
      setTimeout(() => {
        set((state) => ({
          toasts: state.toasts.filter((t) => t.id !== id),
        }));
      }, duration);
    }
  },

  removeToast: (id) =>
    set((state) => ({
      toasts: state.toasts.filter((t) => t.id !== id),
    })),
}));

// Convenience functions callable from anywhere (no hook needed)
export const toast = {
  success: (msg: string, duration?: number) =>
    useToastStore.getState().addToast(msg, "success", duration),
  error: (msg: string, duration?: number) =>
    useToastStore.getState().addToast(msg, "error", duration),
  warning: (msg: string, duration?: number) =>
    useToastStore.getState().addToast(msg, "warning", duration),
  info: (msg: string, duration?: number) =>
    useToastStore.getState().addToast(msg, "info", duration),
};
