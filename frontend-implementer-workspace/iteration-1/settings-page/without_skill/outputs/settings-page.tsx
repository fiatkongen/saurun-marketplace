import { useState } from "react";
import { Loader2, Settings, ChevronDown, Check } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { useSettingsStore } from "./settings-store";
import { useToast } from "./use-toast";
import { ToastContainer } from "./toast";
import { ProfileTab } from "./profile-tab";
import { NotificationsTab } from "./notifications-tab";
import { AppearanceTab } from "./appearance-tab";

type TabKey = "profile" | "notifications" | "appearance";

interface TabConfig {
  key: TabKey;
  label: string;
}

const tabs: TabConfig[] = [
  { key: "profile", label: "Profile" },
  { key: "notifications", label: "Notifications" },
  { key: "appearance", label: "Appearance" },
];

function TabContent({ tab }: { tab: TabKey }) {
  switch (tab) {
    case "profile":
      return <ProfileTab />;
    case "notifications":
      return <NotificationsTab />;
    case "appearance":
      return <AppearanceTab />;
  }
}

export function SettingsPage() {
  const [activeTab, setActiveTab] = useState<TabKey>("profile");
  const isDirty = useSettingsStore((s) => s.isDirty);
  const isSaving = useSettingsStore((s) => s.isSaving);
  const save = useSettingsStore((s) => s.save);
  const discard = useSettingsStore((s) => s.discard);
  const { toasts, addToast, dismissToast } = useToast();

  const activeLabel = tabs.find((t) => t.key === activeTab)!.label;

  async function handleSave() {
    try {
      await save();
      addToast("Settings saved successfully.", "success");
    } catch {
      addToast("Failed to save settings. Please try again.", "error");
    }
  }

  function handleDiscard() {
    discard();
    addToast("Changes discarded.", "info");
  }

  return (
    <div className="min-h-screen bg-background">
      <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 lg:px-8">
        {/* Page header */}
        <div className="mb-8">
          <div className="flex items-center gap-3">
            <Settings className="h-6 w-6 text-muted-foreground" />
            <h1 className="text-2xl font-bold text-foreground">Settings</h1>
          </div>
          <p className="mt-1 text-sm text-muted-foreground ml-9">
            Manage your account preferences.
          </p>
        </div>

        {/* Tab navigation — desktop: horizontal tabs, mobile: dropdown */}
        <div className="mb-8">
          {/* Desktop tabs */}
          <nav className="hidden sm:flex border-b border-border" aria-label="Settings tabs">
            {tabs.map((tab) => {
              const active = activeTab === tab.key;
              return (
                <button
                  key={tab.key}
                  type="button"
                  onClick={() => setActiveTab(tab.key)}
                  className={`relative px-4 py-2.5 text-sm font-medium transition-colors cursor-pointer ${
                    active
                      ? "text-foreground"
                      : "text-muted-foreground hover:text-foreground"
                  }`}
                  aria-selected={active}
                  role="tab"
                >
                  {tab.label}
                  {active && (
                    <span className="absolute bottom-0 left-0 right-0 h-0.5 bg-primary rounded-t-full" />
                  )}
                </button>
              );
            })}
          </nav>

          {/* Mobile dropdown */}
          <div className="sm:hidden">
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button
                  variant="outline"
                  className="w-full justify-between"
                >
                  {activeLabel}
                  <ChevronDown className="ml-2 h-4 w-4 text-muted-foreground" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="start" className="w-[var(--radix-dropdown-menu-trigger-width)]">
                {tabs.map((tab) => (
                  <DropdownMenuItem
                    key={tab.key}
                    onClick={() => setActiveTab(tab.key)}
                    className="flex items-center justify-between"
                  >
                    {tab.label}
                    {activeTab === tab.key && (
                      <Check className="h-4 w-4 text-primary" />
                    )}
                  </DropdownMenuItem>
                ))}
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </div>

        {/* Tab content */}
        <div className="pb-24">
          <TabContent tab={activeTab} />
        </div>
      </div>

      {/* Sticky save bar */}
      {isDirty && (
        <div className="fixed bottom-0 inset-x-0 z-40 border-t border-border bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/80">
          <div className="mx-auto max-w-4xl px-4 py-3 sm:px-6 lg:px-8 flex items-center justify-between gap-4">
            <p className="text-sm text-muted-foreground">
              You have unsaved changes.
            </p>
            <div className="flex items-center gap-3">
              <Button
                variant="ghost"
                size="sm"
                onClick={handleDiscard}
                disabled={isSaving}
              >
                Discard
              </Button>
              <Button
                size="sm"
                onClick={handleSave}
                disabled={isSaving}
              >
                {isSaving ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Saving...
                  </>
                ) : (
                  "Save changes"
                )}
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Toasts */}
      <ToastContainer toasts={toasts} onDismiss={dismissToast} />
    </div>
  );
}
