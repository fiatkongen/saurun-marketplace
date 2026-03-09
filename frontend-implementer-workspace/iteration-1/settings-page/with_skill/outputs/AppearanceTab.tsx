import { cn } from './cn'
import { useAppearance, useAppearanceActions } from './useSettingsStore'
import type { ThemeMode } from './types'

const THEME_OPTIONS: {
  value: ThemeMode
  label: string
  icon: React.ReactNode
  description: string
}[] = [
  {
    value: 'light',
    label: 'Light',
    description: 'Bright and airy',
    icon: (
      <svg width="20" height="20" viewBox="0 0 20 20" fill="none" aria-hidden="true">
        <circle cx="10" cy="10" r="4" stroke="currentColor" strokeWidth="1.5" />
        <path
          d="M10 2v2M10 16v2M2 10h2M16 10h2M4.93 4.93l1.41 1.41M13.66 13.66l1.41 1.41M4.93 15.07l1.41-1.41M13.66 6.34l1.41-1.41"
          stroke="currentColor"
          strokeWidth="1.5"
          strokeLinecap="round"
        />
      </svg>
    ),
  },
  {
    value: 'dark',
    label: 'Dark',
    description: 'Easy on the eyes',
    icon: (
      <svg width="20" height="20" viewBox="0 0 20 20" fill="none" aria-hidden="true">
        <path
          d="M17.5 11.5a7.5 7.5 0 01-9-9 7.5 7.5 0 109 9z"
          stroke="currentColor"
          strokeWidth="1.5"
          strokeLinejoin="round"
        />
      </svg>
    ),
  },
  {
    value: 'system',
    label: 'System',
    description: 'Match your device',
    icon: (
      <svg width="20" height="20" viewBox="0 0 20 20" fill="none" aria-hidden="true">
        <rect x="2" y="3" width="16" height="11" rx="2" stroke="currentColor" strokeWidth="1.5" />
        <path
          d="M7 17h6M10 14v3"
          stroke="currentColor"
          strokeWidth="1.5"
          strokeLinecap="round"
        />
      </svg>
    ),
  },
]

const ACCENT_COLORS = [
  { value: '#6366f1', label: 'Indigo' },
  { value: '#8b5cf6', label: 'Violet' },
  { value: '#ec4899', label: 'Pink' },
  { value: '#f43f5e', label: 'Rose' },
  { value: '#f97316', label: 'Orange' },
  { value: '#eab308', label: 'Yellow' },
  { value: '#22c55e', label: 'Green' },
  { value: '#06b6d4', label: 'Cyan' },
  { value: '#3b82f6', label: 'Blue' },
  { value: '#64748b', label: 'Slate' },
]

const FONT_SIZE_LABELS: Record<number, string> = {
  12: 'Tiny',
  13: 'Small',
  14: 'Compact',
  15: 'Snug',
  16: 'Default',
  17: 'Relaxed',
  18: 'Large',
  19: 'Larger',
  20: 'Huge',
}

export function AppearanceTab() {
  const appearance = useAppearance()
  const { setTheme, setFontSize, setAccentColor } = useAppearanceActions()

  return (
    <div className="space-y-10" data-testid="settings-appearance-tab">
      {/* Section header */}
      <div>
        <h2 className="text-lg font-semibold text-zinc-900 dark:text-zinc-100">
          Appearance
        </h2>
        <p className="mt-1 text-sm text-zinc-500 dark:text-zinc-400">
          Customize the look and feel of your interface.
        </p>
      </div>

      {/* Theme selector */}
      <fieldset>
        <legend className="text-sm font-medium text-zinc-700 dark:text-zinc-300">
          Theme
        </legend>
        <div className="mt-3 grid grid-cols-1 gap-3 sm:grid-cols-3">
          {THEME_OPTIONS.map((option) => (
            <button
              key={option.value}
              data-testid={`settings-appearance-theme-${option.value}`}
              onClick={() => setTheme(option.value)}
              className={cn(
                'group relative flex flex-col items-center gap-2 rounded-xl px-4 py-5',
                'transition-all duration-150',
                'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-zinc-500',
                appearance.theme === option.value
                  ? [
                      'bg-zinc-900 text-white dark:bg-white dark:text-zinc-900',
                      'shadow-md',
                    ]
                  : [
                      'bg-zinc-50 text-zinc-600 dark:bg-zinc-800/60 dark:text-zinc-400',
                      'inset-ring inset-ring-zinc-200 dark:inset-ring-zinc-700',
                      'hover:bg-zinc-100 dark:hover:bg-zinc-800',
                      'hover:text-zinc-900 dark:hover:text-zinc-200',
                    ]
              )}
              aria-pressed={appearance.theme === option.value}
            >
              {option.icon}
              <span className="text-sm font-medium">{option.label}</span>
              <span
                className={cn(
                  'text-xs',
                  appearance.theme === option.value
                    ? 'text-white/70 dark:text-zinc-900/60'
                    : 'text-zinc-400 dark:text-zinc-500'
                )}
              >
                {option.description}
              </span>
            </button>
          ))}
        </div>
      </fieldset>

      {/* Font size slider */}
      <div>
        <div className="flex items-baseline justify-between">
          <label
            htmlFor="settings-font-size"
            className="text-sm font-medium text-zinc-700 dark:text-zinc-300"
          >
            Font size
          </label>
          <span className="text-sm text-zinc-500 dark:text-zinc-400">
            {appearance.fontSize}px
            {FONT_SIZE_LABELS[appearance.fontSize]
              ? ` \u00b7 ${FONT_SIZE_LABELS[appearance.fontSize]}`
              : ''}
          </span>
        </div>

        <div className="mt-3 space-y-3">
          <input
            id="settings-font-size"
            data-testid="settings-appearance-font-size"
            type="range"
            min={12}
            max={20}
            step={1}
            value={appearance.fontSize}
            onChange={(e) => setFontSize(Number(e.target.value))}
            className={cn(
              'w-full cursor-pointer appearance-none rounded-full h-1.5',
              'bg-zinc-200 dark:bg-zinc-700',
              'accent-zinc-900 dark:accent-zinc-100',
              '[&::-webkit-slider-thumb]:h-4 [&::-webkit-slider-thumb]:w-4',
              '[&::-webkit-slider-thumb]:appearance-none [&::-webkit-slider-thumb]:rounded-full',
              '[&::-webkit-slider-thumb]:bg-zinc-900 dark:[&::-webkit-slider-thumb]:bg-zinc-100',
              '[&::-webkit-slider-thumb]:shadow-sm',
              '[&::-webkit-slider-thumb]:transition-transform',
              '[&::-webkit-slider-thumb]:hover:scale-125'
            )}
            aria-valuemin={12}
            aria-valuemax={20}
            aria-valuenow={appearance.fontSize}
            aria-valuetext={`${appearance.fontSize} pixels${
              FONT_SIZE_LABELS[appearance.fontSize]
                ? `, ${FONT_SIZE_LABELS[appearance.fontSize]}`
                : ''
            }`}
          />

          {/* Preview */}
          <div
            data-testid="settings-appearance-font-preview"
            className={cn(
              'rounded-lg px-4 py-3',
              'bg-zinc-50 dark:bg-zinc-800/60',
              'inset-ring inset-ring-zinc-100 dark:inset-ring-zinc-700/50',
              'text-zinc-700 dark:text-zinc-300'
            )}
            style={{ fontSize: `${appearance.fontSize}px` }}
          >
            The quick brown fox jumps over the lazy dog.
          </div>
        </div>
      </div>

      {/* Accent color picker */}
      <fieldset>
        <legend className="text-sm font-medium text-zinc-700 dark:text-zinc-300">
          Accent color
        </legend>
        <div className="mt-3 flex flex-wrap gap-2.5">
          {ACCENT_COLORS.map((color) => (
            <button
              key={color.value}
              data-testid={`settings-appearance-accent-${color.label.toLowerCase()}`}
              onClick={() => setAccentColor(color.value)}
              className={cn(
                'group relative h-9 w-9 rounded-full transition-all duration-150',
                'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-zinc-500',
                appearance.accentColor === color.value
                  ? 'ring-2 ring-offset-2 ring-offset-white dark:ring-offset-zinc-900'
                  : 'hover:scale-110'
              )}
              style={{
                backgroundColor: color.value,
                ...(appearance.accentColor === color.value
                  ? { ringColor: color.value }
                  : {}),
              }}
              aria-label={`${color.label} accent color`}
              aria-pressed={appearance.accentColor === color.value}
            >
              {/* Checkmark for selected */}
              {appearance.accentColor === color.value && (
                <svg
                  width="16"
                  height="16"
                  viewBox="0 0 16 16"
                  fill="none"
                  className="absolute inset-0 m-auto text-white drop-shadow-sm"
                  aria-hidden="true"
                >
                  <path
                    d="M3.5 8.5L6.5 11.5L12.5 5.5"
                    stroke="currentColor"
                    strokeWidth="2"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  />
                </svg>
              )}
            </button>
          ))}
        </div>
        <p className="mt-2 text-xs text-zinc-400 dark:text-zinc-500">
          {ACCENT_COLORS.find((c) => c.value === appearance.accentColor)?.label ??
            'Custom'}{' '}
          selected
        </p>
      </fieldset>
    </div>
  )
}
