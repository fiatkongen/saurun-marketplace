import { cn } from './cn'
import { useToast, useDismissToast } from './useSettingsStore'

export function Toast() {
  const toast = useToast()
  const dismiss = useDismissToast()

  if (!toast) return null

  return (
    <div
      data-testid="settings-toast"
      role="alert"
      aria-live="polite"
      className={cn(
        'fixed bottom-6 right-6 z-50 flex items-center gap-3 rounded-lg px-5 py-3.5',
        'shadow-lg animate-in slide-in-from-bottom-4 fade-in duration-300',
        'font-[Geist,sans-serif] text-sm tracking-tight',
        toast.type === 'success' &&
          'bg-emerald-950 text-emerald-100 inset-ring inset-ring-emerald-500/20',
        toast.type === 'error' &&
          'bg-red-950 text-red-100 inset-ring inset-ring-red-500/20'
      )}
    >
      <span
        className={cn(
          'flex h-5 w-5 shrink-0 items-center justify-center rounded-full text-xs',
          toast.type === 'success' && 'bg-emerald-500/20 text-emerald-400',
          toast.type === 'error' && 'bg-red-500/20 text-red-400'
        )}
      >
        {toast.type === 'success' ? (
          <svg width="12" height="12" viewBox="0 0 12 12" fill="none" aria-hidden="true">
            <path
              d="M2.5 6.5L4.5 8.5L9.5 3.5"
              stroke="currentColor"
              strokeWidth="1.5"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
        ) : (
          <svg width="12" height="12" viewBox="0 0 12 12" fill="none" aria-hidden="true">
            <path
              d="M3.5 3.5L8.5 8.5M8.5 3.5L3.5 8.5"
              stroke="currentColor"
              strokeWidth="1.5"
              strokeLinecap="round"
            />
          </svg>
        )}
      </span>

      <span>{toast.message}</span>

      <button
        data-testid="settings-toast-dismiss"
        onClick={dismiss}
        className="ml-2 rounded p-0.5 opacity-60 transition-opacity hover:opacity-100 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-current"
        aria-label="Dismiss notification"
      >
        <svg width="14" height="14" viewBox="0 0 14 14" fill="none" aria-hidden="true">
          <path
            d="M4 4L10 10M10 4L4 10"
            stroke="currentColor"
            strokeWidth="1.5"
            strokeLinecap="round"
          />
        </svg>
      </button>
    </div>
  )
}
