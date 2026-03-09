import { cn } from './cn'
import { useIsDirty, useIsSaving, useSaveActions } from './useSettingsStore'

export function StickySaveBar() {
  const isDirty = useIsDirty()
  const isSaving = useIsSaving()
  const { save, discard } = useSaveActions()

  return (
    <div
      data-testid="settings-save-bar"
      className={cn(
        'fixed bottom-0 inset-x-0 z-40 transition-all duration-300 ease-out',
        'font-[Geist,sans-serif]',
        isDirty
          ? 'translate-y-0 opacity-100'
          : 'translate-y-full opacity-0 pointer-events-none'
      )}
    >
      <div className="mx-auto max-w-3xl px-4 pb-4 sm:px-6">
        <div
          className={cn(
            'flex items-center justify-between gap-4 rounded-xl px-5 py-3.5',
            'bg-zinc-900/95 text-zinc-100 backdrop-blur-sm',
            'shadow-2xl inset-ring inset-ring-white/10'
          )}
        >
          <p className="text-sm text-zinc-400 hidden sm:block">
            You have unsaved changes
          </p>
          <p className="text-sm text-zinc-400 sm:hidden">
            Unsaved changes
          </p>

          <div className="flex items-center gap-2.5">
            <button
              data-testid="settings-save-bar-discard"
              onClick={discard}
              disabled={isSaving}
              className={cn(
                'rounded-lg px-4 py-2 text-sm font-medium transition-colors',
                'text-zinc-300 hover:bg-white/10 hover:text-white',
                'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-zinc-400',
                'disabled:opacity-40 disabled:cursor-not-allowed'
              )}
            >
              Discard
            </button>

            <button
              data-testid="settings-save-bar-save"
              onClick={save}
              disabled={isSaving}
              className={cn(
                'rounded-lg px-5 py-2 text-sm font-semibold transition-all',
                'bg-white text-zinc-900 hover:bg-zinc-100',
                'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white',
                'disabled:opacity-60 disabled:cursor-not-allowed',
                isSaving && 'animate-pulse'
              )}
            >
              {isSaving ? (
                <span className="flex items-center gap-2">
                  <svg
                    className="h-3.5 w-3.5 animate-spin"
                    viewBox="0 0 16 16"
                    fill="none"
                    aria-hidden="true"
                  >
                    <circle
                      cx="8"
                      cy="8"
                      r="6"
                      stroke="currentColor"
                      strokeWidth="2"
                      strokeDasharray="28"
                      strokeDashoffset="8"
                      strokeLinecap="round"
                    />
                  </svg>
                  Saving...
                </span>
              ) : (
                'Save changes'
              )}
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
