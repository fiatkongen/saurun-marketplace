import { useRef } from "react";
import { Camera, User } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { useSettingsStore } from "./settings-store";

export function ProfileTab() {
  const profile = useSettingsStore((s) => s.draft.profile);
  const updateProfile = useSettingsStore((s) => s.updateProfile);
  const fileInputRef = useRef<HTMLInputElement>(null);

  function handleAvatarUpload(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (ev) => {
      updateProfile({ avatarUrl: ev.target?.result as string });
    };
    reader.readAsDataURL(file);
  }

  return (
    <div className="space-y-8">
      <div>
        <h2 className="text-lg font-semibold text-foreground">Profile</h2>
        <p className="text-sm text-muted-foreground mt-1">
          Manage your public profile information.
        </p>
      </div>

      {/* Avatar */}
      <div className="flex items-center gap-6">
        <div className="relative group">
          <Avatar className="h-20 w-20">
            <AvatarImage src={profile.avatarUrl ?? undefined} alt={profile.name} />
            <AvatarFallback className="text-xl">
              <User className="h-8 w-8" />
            </AvatarFallback>
          </Avatar>
          <button
            type="button"
            onClick={() => fileInputRef.current?.click()}
            className="absolute inset-0 flex items-center justify-center rounded-full bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity cursor-pointer"
            aria-label="Upload avatar"
          >
            <Camera className="h-5 w-5 text-white" />
          </button>
          <input
            ref={fileInputRef}
            type="file"
            accept="image/*"
            className="hidden"
            onChange={handleAvatarUpload}
          />
        </div>
        <div className="space-y-1">
          <Button
            variant="outline"
            size="sm"
            onClick={() => fileInputRef.current?.click()}
          >
            Upload photo
          </Button>
          <p className="text-xs text-muted-foreground">
            JPG, PNG or GIF. Max 2MB.
          </p>
        </div>
      </div>

      {/* Name */}
      <div className="grid gap-2">
        <Label htmlFor="profile-name">Full name</Label>
        <Input
          id="profile-name"
          value={profile.name}
          onChange={(e) => updateProfile({ name: e.target.value })}
          placeholder="Jane Doe"
          className="max-w-md"
        />
      </div>

      {/* Email */}
      <div className="grid gap-2">
        <Label htmlFor="profile-email">Email address</Label>
        <Input
          id="profile-email"
          type="email"
          value={profile.email}
          onChange={(e) => updateProfile({ email: e.target.value })}
          placeholder="jane@example.com"
          className="max-w-md"
        />
      </div>

      {/* Bio */}
      <div className="grid gap-2">
        <Label htmlFor="profile-bio">Bio</Label>
        <Textarea
          id="profile-bio"
          value={profile.bio}
          onChange={(e) => updateProfile({ bio: e.target.value })}
          placeholder="Tell us about yourself..."
          rows={4}
          className="max-w-lg resize-none"
        />
        <p className="text-xs text-muted-foreground">
          {profile.bio.length}/300 characters
        </p>
      </div>
    </div>
  );
}
