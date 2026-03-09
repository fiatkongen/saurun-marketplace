import { useCallback, useRef } from 'react'
import { cn } from './cn'
import { useProfile, useProfileActions } from './useSettingsStore'

export function ProfileTab() {
  const profile = useProfile()
  const setField = useProfileActions()
  const fileInputRef = useRef<HTMLInputElement>(null)

  const handleAvatarClick = useCallback(() => {
    fileInputRef.current?.click()
  }, [])

  const handleAvatarChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const file = e.target.files?.[0]
      if (file) {
        const url = URL.createObjectURL(file)
        setField('avatarUrl', url)
      }
    },
    [setField]
  )

  return (
    <div className="space-y-8" data-testid="settings-profile-tab">
      {/* Section header */}
      <div>
        <h2 className="text-lg font-semibold text-zinc-900 dark:text-zinc-100">
          Profile
        </h2>
        <p className="mt-1 text-sm text-zinc-500 dark:text-zinc-400">
          Your public-facing identity. This information may be visible to others.
        </p>
      </div>

      {/* Avatar */}
      <div className="flex items-center gap-6">
        <button
          data-testid="settings-profile-avatar-upload"
          onClick={handleAvatarClick}
          className={cn(
            'group relative h-20 w-20 shrink-0 overflow-hidden rounded-2xl transition-all',
            'bg-zinc-100 dark:bg-zinc-800',
            'inset-ring inset-ring-zinc-200 dark:inset-ring-zinc-700',
            'hover:inset-ring-zinc-400 dark:hover:inset-ring-zinc-500',
            'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-zinc-500'
          )}
          type="button"
          aria-label="Upload avatar"
        >
          {profile.avatarUrl ? (
            <img
              src={profile.avatarUrl}
              alt="Avatar"
              className="h-full w-full object-cover"
            />
          ) : (
            <div className="flex h-full w-full items-center justify-center">
              <svg
                width="28"
                height="28"
                viewBox="0 0 28 28"
                fill="none"
                className="text-zinc-400 dark:text-zinc-500"
                aria-hidden="true"
              >
                <circle cx="14" cy="10" r="5" stroke="currentColor" strokeWidth="1.5" />
                <path
                  d="M4 24c0-4.418 4.477-8 10-8s10 3.582 10 8"
                  stroke="currentColor"
                  strokeWidth="1.5"
                  strokeLinecap="round"
                />
              </svg>
            </div>
          )}

          {/* Overlay on hover */}
          <div
            className={cn(
              'absolute inset-0 flex items-center justify-center',
              'bg-black/40 opacity-0 transition-opacity',
              'group-hover:opacity-100'
            )}
          >
            <svg
              width="18"
              height="18"
              viewBox="0 0 18 18"
              fill="none"
              className="text-white"
              aria-hidden="true"
            >
              <path
                d="M6.5 2.5L5.5 4H3a1 1 0 00-1 1v9a1 1 0 001 1h12a1 1 0 001-1V5a1 1 0 00-1-1h-2.5l-1-1.5h-5z"
                stroke="currentColor"
                strokeWidth="1.2"
              />
              <circle cx="9" cy="9" r="3" stroke="currentColor" strokeWidth="1.2" />
            </svg>
          </div>
        </button>

        <input
          ref={fileInputRef}
          data-testid="settings-profile-avatar-input"
          type="file"
          accept="image/*"
          onChange={handleAvatarChange}
          className="sr-only"
          tabIndex={-1}
          aria-label="Choose avatar file"
        />

        <div className="text-sm">
          <p className="font-medium text-zinc-700 dark:text-zinc-300">
            Profile photo
          </p>
          <p className="mt-0.5 text-zinc-500 dark:text-zinc-400">
            JPG, PNG, or GIF. Max 4MB.
          </p>
        </div>
      </div>

      {/* Name + Email row */}
      <div className="grid gap-6 sm:grid-cols-2">
        <div>
          <label
            htmlFor="settings-name"
            className="block text-sm font-medium text-zinc-700 dark:text-zinc-300"
          >
            Full name
          </label>
          <input
            id="settings-name"
            data-testid="settings-profile-name"
            type="text"
            value={profile.name}
            onChange={(e) => setField('name', e.target.value)}
            placeholder="Jane Doe"
            className={cn(
              'mt-1.5 block w-full rounded-lg border-0 px-3.5 py-2.5 text-sm',
              'bg-white dark:bg-zinc-800/60',
              'text-zinc-900 dark:text-zinc-100',
              'inset-ring inset-ring-zinc-200 dark:inset-ring-zinc-700',
              'placeholder:text-zinc-400 dark:placeholder:text-zinc-500',
              'transition-all',
              'hover:inset-ring-zinc-300 dark:hover:inset-ring-zinc-600',
              'focus:inset-ring-2 focus:inset-ring-zinc-900 dark:focus:inset-ring-zinc-300',
              'focus:outline-none'
            )}
          />
        </div>

        <div>
          <label
            htmlFor="settings-email"
            className="block text-sm font-medium text-zinc-700 dark:text-zinc-300"
          >
            Email address
          </label>
          <input
            id="settings-email"
            data-testid="settings-profile-email"
            type="email"
            value={profile.email}
            onChange={(e) => setField('email', e.target.value)}
            placeholder="jane@example.com"
            className={cn(
              'mt-1.5 block w-full rounded-lg border-0 px-3.5 py-2.5 text-sm',
              'bg-white dark:bg-zinc-800/60',
              'text-zinc-900 dark:text-zinc-100',
              'inset-ring inset-ring-zinc-200 dark:inset-ring-zinc-700',
              'placeholder:text-zinc-400 dark:placeholder:text-zinc-500',
              'transition-all',
              'hover:inset-ring-zinc-300 dark:hover:inset-ring-zinc-600',
              'focus:inset-ring-2 focus:inset-ring-zinc-900 dark:focus:inset-ring-zinc-300',
              'focus:outline-none'
            )}
          />
        </div>
      </div>

      {/* Bio */}
      <div>
        <label
          htmlFor="settings-bio"
          className="block text-sm font-medium text-zinc-700 dark:text-zinc-300"
        >
          Bio
        </label>
        <textarea
          id="settings-bio"
          data-testid="settings-profile-bio"
          value={profile.bio}
          onChange={(e) => setField('bio', e.target.value)}
          placeholder="Tell us about yourself..."
          rows={4}
          className={cn(
            'mt-1.5 block w-full rounded-lg border-0 px-3.5 py-2.5 text-sm',
            'bg-white dark:bg-zinc-800/60',
            'text-zinc-900 dark:text-zinc-100',
            'inset-ring inset-ring-zinc-200 dark:inset-ring-zinc-700',
            'placeholder:text-zinc-400 dark:placeholder:text-zinc-500',
            'transition-all resize-none',
            'hover:inset-ring-zinc-300 dark:hover:inset-ring-zinc-600',
            'focus:inset-ring-2 focus:inset-ring-zinc-900 dark:focus:inset-ring-zinc-300',
            'focus:outline-none'
          )}
        />
        <p className="mt-1.5 text-xs text-zinc-400 dark:text-zinc-500">
          {profile.bio.length}/280 characters
        </p>
      </div>
    </div>
  )
}
