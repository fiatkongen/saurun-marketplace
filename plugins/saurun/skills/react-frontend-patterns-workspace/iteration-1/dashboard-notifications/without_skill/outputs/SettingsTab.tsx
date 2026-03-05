// ============================================================
// SettingsTab.tsx — Settings tab content
// ============================================================

import React, { useState } from "react";
import { toast } from "./toast-store";

export function SettingsTab() {
  const [emailNotifications, setEmailNotifications] = useState(true);
  const [darkMode, setDarkMode] = useState(false);

  const handleSave = () => {
    // Simulated save
    toast.success("Settings saved successfully");
  };

  return (
    <div className="max-w-lg space-y-6">
      <div className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm space-y-4">
        <h3 className="text-lg font-semibold text-gray-900">Preferences</h3>

        <label className="flex items-center justify-between cursor-pointer">
          <span className="text-sm text-gray-700">Email notifications</span>
          <input
            type="checkbox"
            checked={emailNotifications}
            onChange={(e) => setEmailNotifications(e.target.checked)}
            className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
          />
        </label>

        <label className="flex items-center justify-between cursor-pointer">
          <span className="text-sm text-gray-700">Dark mode</span>
          <input
            type="checkbox"
            checked={darkMode}
            onChange={(e) => setDarkMode(e.target.checked)}
            className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
          />
        </label>
      </div>

      <button
        onClick={handleSave}
        className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 transition-colors"
      >
        Save Settings
      </button>
    </div>
  );
}
