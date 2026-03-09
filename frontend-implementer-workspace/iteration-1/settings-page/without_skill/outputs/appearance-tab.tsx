import { Monitor, Moon, Sun } from "lucide-react";
import { Label } from "@/components/ui/label";
import { Slider } from "@/components/ui/slider";
import { useSettingsStore, type ThemeMode } from "./settings-store";

const themes: { value: ThemeMode; label: string; icon: React.ReactNode }[] = [
  { value: "light", label: "Light", icon: <Sun className="h-5 w-5" /> },
  { value: "dark", label: "Dark", icon: <Moon className="h-5 w-5" /> },
  { value: "system", label: "System", icon: <Monitor className="h-5 w-5" /> },
];

const accentColors = [
  { value: "#6366f1", label: "Indigo" },
  { value: "#8b5cf6", label: "Violet" },
  { value: "#ec4899", label: "Pink" },
  { value: "#f43f5e", label: "Rose" },
  { value: "#f97316", label: "Orange" },
  { value: "#eab308", label: "Yellow" },
  { value: "#22c55e", label: "Green" },
  { value: "#06b6d4", label: "Cyan" },
  { value: "#3b82f6", label: "Blue" },
  { value: "#64748b", label: "Slate" },
];

export function AppearanceTab() {
  const appearance = useSettingsStore((s) => s.draft.appearance);
  const updateAppearance = useSettingsStore((s) => s.updateAppearance);

  return (
    <div className="space-y-10">
      <div>
        <h2 className="text-lg font-semibold text-foreground">Appearance</h2>
        <p className="text-sm text-muted-foreground mt-1">
          Customize how the app looks and feels.
        </p>
      </div>

      {/* Theme toggle */}
      <div className="space-y-3">
        <Label className="text-sm font-medium">Theme</Label>
        <div className="grid grid-cols-3 gap-3 max-w-sm">
          {themes.map((t) => {
            const active = appearance.theme === t.value;
            return (
              <button
                key={t.value}
                type="button"
                onClick={() => updateAppearance({ theme: t.value })}
                className={`flex flex-col items-center gap-2 rounded-lg border-2 px-4 py-3 transition-colors cursor-pointer ${
                  active
                    ? "border-primary bg-primary/5 text-primary"
                    : "border-border text-muted-foreground hover:border-muted-foreground/40 hover:bg-muted/50"
                }`}
                aria-pressed={active}
              >
                {t.icon}
                <span className="text-xs font-medium">{t.label}</span>
              </button>
            );
          })}
        </div>
      </div>

      {/* Font size slider */}
      <div className="space-y-4 max-w-sm">
        <div className="flex items-center justify-between">
          <Label className="text-sm font-medium">Font size</Label>
          <span className="text-sm tabular-nums text-muted-foreground">
            {appearance.fontSize}px
          </span>
        </div>
        <Slider
          min={12}
          max={24}
          step={1}
          value={[appearance.fontSize]}
          onValueChange={([v]) => updateAppearance({ fontSize: v })}
        />
        <div className="flex justify-between text-xs text-muted-foreground">
          <span>Small</span>
          <span>Default</span>
          <span>Large</span>
        </div>
        {/* Preview */}
        <p
          className="mt-2 rounded-md border border-border bg-muted/30 p-3 text-foreground"
          style={{ fontSize: `${appearance.fontSize}px` }}
        >
          The quick brown fox jumps over the lazy dog.
        </p>
      </div>

      {/* Accent color picker */}
      <div className="space-y-3">
        <Label className="text-sm font-medium">Accent color</Label>
        <div className="flex flex-wrap gap-3">
          {accentColors.map((color) => {
            const active = appearance.accentColor === color.value;
            return (
              <button
                key={color.value}
                type="button"
                onClick={() => updateAppearance({ accentColor: color.value })}
                className={`group relative h-9 w-9 rounded-full border-2 transition-all cursor-pointer ${
                  active
                    ? "border-foreground scale-110"
                    : "border-transparent hover:scale-105"
                }`}
                style={{ backgroundColor: color.value }}
                aria-label={color.label}
                aria-pressed={active}
                title={color.label}
              >
                {active && (
                  <svg
                    className="absolute inset-0 m-auto h-4 w-4 text-white drop-shadow-sm"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                    strokeWidth={3}
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      d="M5 13l4 4L19 7"
                    />
                  </svg>
                )}
              </button>
            );
          })}
        </div>
        {/* Color preview bar */}
        <div
          className="h-2 w-full max-w-sm rounded-full mt-2 transition-colors"
          style={{ backgroundColor: appearance.accentColor }}
        />
      </div>
    </div>
  );
}
