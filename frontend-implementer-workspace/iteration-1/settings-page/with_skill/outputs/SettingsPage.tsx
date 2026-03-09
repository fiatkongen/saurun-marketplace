import { useState, useCallback, useRef, useEffect } from 'react'
import { cn } from './cn'
import {
  useActiveTab,
  useSetActiveTab,
} from './useSettingsStore'
import { ProfileTab } from './ProfileTab'
import { NotificationsTab } from './NotificationsTab'
import { AppearanceTab } from './AppearanceTab'
import { StickySaveBar } from './StickySaveBar'
import { Toast } from './Toast'
import type { SettingsTab } from './types'

// ============================================
// Tab Configuration
// ============================================

const TABS: {
  id: SettingsTab
  label: string
  icon: React.ReactNode
}[] = [
  {
    id: 'profile',
    label: 'Profile',
    icon: (
      <svg width="18" height="18" viewBox="0 0 18 18" fill="none" aria-hidden="true">
        <circle cx="9" cy="6.5" r="3.5" stroke="currentColor" strokeWidth="1.3" />
        <path
          d="M2.5 16c0-3.314 2.91-6 6.5-6s6.5 2.686 6.5 6"
          stroke="currentColor"
          strokeWidth="1.3"
          strokeLinecap="round"
        />
      </svg>
    ),
  },
  {
    id: 'notifications',
    label: 'Notifications',
    icon: (
      <svg width="18" height="18" viewBox="0 0 18 18" fill="none" aria-hidden="true">
        <path
          d="M6 13.5a3 3 0 006 0M9 1.5a5 5 0 00-5 5c0 2.5-1.5 4.5-1.5 4.5h13S14 9 14 6.5a5 5 0 00-5-5z"
          stroke="currentColor"
          strokeWidth="1.3"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
      </svg>
    ),
  },
  {
    id: 'appearance',
    label: 'Appearance',
    icon: (
      <svg width="18" height="18" viewBox="0 0 18 18" fill="none" aria-hidden="true">
        <circle cx="9" cy="9" r="7" stroke="currentColor" strokeWidth="1.3" />
        <path d="M9 2v14" stroke="currentColor" strokeWidth="1.3" />
        <path
          d="M9 2a7 7 0 010 14"
          fill="currentColor"
          fillOpacity="0.15"
        />
      </svg>
    ),
  },
]

// ============================================
// Mobile Dropdown
// ============================================

function MobileTabSelect() {
  const activeTab = useActiveTab()
  const setActiveTab = useSetActiveTab()
  const [isOpen, setIsOpen] = useState(false)
  const dropdownRef = useRef<HTMLDivElement>(null)

  const activeOption = TABS.find((t) => t.id === activeTab)

  const handleSelect = useCallback(
    (tab: SettingsTab) => {
      setActiveTab(tab)
      setIsOpen(false)
    },
    [setActiveTab]
  )

  // Close on outside click
  useEffect(() => {
    if (!isOpen) return
    function handleClick(e: MouseEvent) {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(e.target as Node)
      ) {
        setIsOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClick)
    return () => document.removeEventListener('mousedown', handleClick)
  }, [isOpen])

  return (
    <div ref={dropdownRef} className="relative sm:hidden">
      <button
        data-testid="settings-mobile-tab-select"
        onClick={() => setIsOpen((o) => !o)}
        className={cn(
          'flex w-full items-center justify-between rounded-lg px-4 py-3 text-sm font-medium',
          'bg-white dark:bg-zinc-800/60',
          'text-zinc-900 dark:text-zinc-100',
          'inset-ring inset-ring-zinc-200 dark:inset-ring-zinc-700',
          'transition-all',
          'hover:bg-zinc-50 dark:hover:bg-zinc-800',
          'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-zinc-500'
        )}
        aria-expanded={isOpen}
        aria-haspopup="listbox"
      >
        <span className="flex items-center gap-2.5">
          <span className="text-zinc-500 dark:text-zinc-400">
            {activeOption?.icon}
          </span>
          {activeOption?.label}
        </span>

        <svg
          width="16"
          height="16"
          viewBox="0 0 16 16"
          fill="none"
          className={cn(
            'text-zinc-400 transition-transform duration-200',
            isOpen && 'rotate-180'
          )}
          aria-hidden="true"
        >
          <path
            d="M4 6l4 4 4-4"
            stroke="currentColor"
            strokeWidth="1.5"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      </button>

      {/* Dropdown menu */}
      {isOpen && (
        <div
          role="listbox"
          className={cn(
            'absolute left-0 right-0 top-full z-30 mt-1.5',
            'rounded-lg overflow-hidden',
            'bg-white dark:bg-zinc-800',
            'shadow-lg inset-ring inset-ring-zinc-200 dark:inset-ring-zinc-700',
            'animate-in fade-in slide-in-from-top-1 duration-150'
          )}
        >
          {TABS.map((tab) => (
            <button
              key={tab.id}
              data-testid={`settings-mobile-tab-option-${tab.id}`}
              role="option"
              aria-selected={activeTab === tab.id}
              onClick={() => handleSelect(tab.id)}
              className={cn(
                'flex w-full items-center gap-2.5 px-4 py-3 text-sm text-left transition-colors',
                activeTab === tab.id
                  ? 'bg-zinc-100 text-zinc-900 dark:bg-zinc-700 dark:text-zinc-100'
                  : 'text-zinc-600 hover:bg-zinc-50 dark:text-zinc-400 dark:hover:bg-zinc-700/50'
              )}
            >
              <span
                className={cn(
                  activeTab === tab.id
                    ? 'text-zinc-900 dark:text-zinc-100'
                    : 'text-zinc-400 dark:text-zinc-500'
                )}
              >
                {tab.icon}
              </span>
              {tab.label}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}

// ============================================
// Desktop Tabs
// ============================================

function DesktopTabs() {
  const activeTab = useActiveTab()
  const setActiveTab = useSetActiveTab()

  return (
    <nav
      className="hidden sm:flex sm:gap-1"
      role="tablist"
      aria-label="Settings sections"
    >
      {TABS.map((tab) => (
        <button
          key={tab.id}
          data-testid={`settings-tab-${tab.id}`}
          role="tab"
          aria-selected={activeTab === tab.id}
          aria-controls={`settings-panel-${tab.id}`}
          onClick={() => setActiveTab(tab.id)}
          className={cn(
            'flex items-center gap-2 rounded-lg px-4 py-2.5 text-sm font-medium',
            'transition-all duration-150',
            'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-zinc-500',
            activeTab === tab.id
              ? [
                  'bg-zinc-900 text-white dark:bg-white dark:text-zinc-900',
                  'shadow-sm',
                ]
              : [
                  'text-zinc-500 dark:text-zinc-400',
                  'hover:bg-zinc-100 hover:text-zinc-900',
                  'dark:hover:bg-zinc-800 dark:hover:text-zinc-200',
                ]
          )}
        >
          {tab.icon}
          {tab.label}
        </button>
      ))}
    </nav>
  )
}

// ============================================
// Settings Page
// ============================================

export function SettingsPage() {
  const activeTab = useActiveTab()

  return (
    <div
      className={cn(
        'min-h-screen',
        'bg-zinc-50 dark:bg-zinc-950',
        'font-[Geist,system-ui,sans-serif]'
      )}
    >
      {/* Background texture */}
      <div
        className="pointer-events-none fixed inset-0 opacity-[0.015] dark:opacity-[0.03]"
        style={{
          backgroundImage:
            'radial-gradient(circle at 1px 1px, currentColor 1px, transparent 0)',
          backgroundSize: '32px 32px',
        }}
        aria-hidden="true"
      />

      <div className="relative mx-auto max-w-3xl px-4 py-10 sm:px-6 sm:py-16">
        {/* Page header */}
        <div className="mb-8 sm:mb-10">
          <h1
            className="text-2xl font-bold tracking-tight text-zinc-900 dark:text-zinc-100 sm:text-3xl"
            data-testid="settings-page-title"
          >
            Settings
          </h1>
          <p className="mt-1.5 text-sm text-zinc-500 dark:text-zinc-400">
            Manage your account preferences and configurations.
          </p>
        </div>

        {/* Tab navigation */}
        <div className="mb-8">
          <DesktopTabs />
          <MobileTabSelect />
        </div>

        {/* Tab panels */}
        <div
          className={cn(
            'rounded-2xl p-6 sm:p-8',
            'bg-white dark:bg-zinc-900',
            'shadow-xs',
            'inset-ring inset-ring-zinc-100 dark:inset-ring-zinc-800',
            // Extra bottom padding when save bar is visible
            'pb-24 sm:pb-8'
          )}
        >
          <div
            id="settings-panel-profile"
            role="tabpanel"
            aria-labelledby="settings-tab-profile"
            hidden={activeTab !== 'profile'}
          >
            {activeTab === 'profile' && <ProfileTab />}
          </div>

          <div
            id="settings-panel-notifications"
            role="tabpanel"
            aria-labelledby="settings-tab-notifications"
            hidden={activeTab !== 'notifications'}
          >
            {activeTab === 'notifications' && <NotificationsTab />}
          </div>

          <div
            id="settings-panel-appearance"
            role="tabpanel"
            aria-labelledby="settings-tab-appearance"
            hidden={activeTab !== 'appearance'}
          >
            {activeTab === 'appearance' && <AppearanceTab />}
          </div>
        </div>
      </div>

      {/* Sticky save bar */}
      <StickySaveBar />

      {/* Toast notifications */}
      <Toast />
    </div>
  )
}
