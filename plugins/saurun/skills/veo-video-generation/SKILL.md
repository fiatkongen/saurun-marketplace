---
name: veo-video-generation
description: Use when generating videos with Google Veo 3.1, writing video prompts, or creating image-to-video animations. Triggers on "generate video", "veo", "video generation", "animate image", "text-to-video".
user-invocable: true
argument-hint: "[prompt text or --help for options]"
allowed-tools: "Bash(*), Read, Write, Glob, Grep"
---

# Veo 3.1 Video Generation

Generate videos via Google's Veo 3.1 API. Includes prompt engineering guide and executable script.

## Usage

```bash
/veo-video-generation A slow dolly shot toward an artisan coffee cup, warm morning light
```

When invoked with arguments, generate the video directly. When invoked without, help the user craft a prompt using the guide below.

## Quick Start

```bash
# Text-to-video
~/.claude/skills/veo-video-generation/generate-video.sh \
  --prompt "Slow tracking shot following a chef plating a dessert, warm kitchen lighting, cinematic"

# Image-to-video
~/.claude/skills/veo-video-generation/generate-video.sh \
  --prompt "Subtle breathing, slow blink, magical sparkles rise from hands" \
  --image ./character.png \
  --duration 8 --resolution 1080p
```

**Requires:** `GEMINI_API_KEY` env var. Cost: ~$0.35/sec (~$2.80 for 8s).

Run `generate-video.sh --help` for all options.

## Prompt Formula

```
[Cinematography] + [Subject] + [Action] + [Context] + [Style & Ambiance]
```

**Always lead with cinematography** — it's the strongest lever.

| Component | Examples |
|-----------|----------|
| **Cinematography** | Slow tracking shot, crane shot rising, extreme close-up, shallow depth of field |
| **Subject** | A tired corporate worker, a fluffy orange tabby cat, a young female explorer |
| **Action** | rubbing his temples, batting playfully at yarn, stepping cautiously forward |
| **Context** | in a cluttered 1980s office lit by fluorescent lights, in a mist-filled canyon at sunrise |
| **Style & Ambiance** | Retro film grain, warm magical atmosphere, cinematic color grading |

## Camera Movement Vocabulary

| Movement | Use for |
|----------|---------|
| **Dolly shot** | Move toward/away from subject — builds tension or reveals |
| **Tracking shot** | Follow subject laterally — journey, progression |
| **Crane shot** | Vertical movement — reveals scale, establishes setting |
| **Arc shot** | Circle around subject — dramatic emphasis |
| **Aerial view** | Bird's eye — context, scale |
| **Slow pan** | Horizontal rotation — survey environment |
| **POV shot** | Character's viewpoint — immersion |

**Shot types:** wide (environment), medium (conversational), close-up (emotion), extreme close-up (detail).

## Audio Direction (Veo 3.1)

Veo 3.1 generates synchronized audio. Include cues:

```
# Dialogue — use quotation marks
A woman turns to camera and says, "We have to leave now."

# Sound effects
SFX: thunder cracks in the distance

# Ambient
Ambient noise: quiet hum of a starship bridge

# Music
SFX: A swelling, gentle orchestral score begins to play.
```

## Silent Scenes

Front-load the constraint + describe closed mouth explicitly:

```
SILENT SCENE - no talking, no dialogue, no speech.
[Subject] has a warm, closed-mouth smile - lips stay together,
mouth does not open or move at any point.
Audio: soft ambient sounds only. No voices, no dialogue, no speech sounds.
```

Avoid implicit triggers: "talking", "chatting", "greeting". Use: "beckoning", "gesturing", "nodding".

## Advanced Workflows

### Timestamp Prompting (multi-shot in single generation)

```
[00:00-00:02] Medium shot from behind a young explorer approaching a canyon edge...
[00:02-00:04] Reverse shot of the explorer's face, eyes widening...
[00:04-00:06] Tracking shot following the explorer stepping forward...
[00:06-00:08] Wide crane shot pulling up and away, revealing the full canyon...
```

### Character Consistency (up to 3 reference images)

Generate reference images first, then pass them to maintain consistent characters across shots. Use `--image` flag.

### First/Last Frame Transitions

1. Generate start frame (e.g., with Gemini image gen)
2. Generate end frame
3. Animate transition with Veo describing the arc between states

## API Parameters

| Parameter | Options | Default | Notes |
|-----------|---------|---------|-------|
| `--prompt` | Any text | **Required** | Video generation directive |
| `--image` | File path | None | .png/.jpg/.jpeg/.webp for image-to-video |
| `--model` | `veo-3.1-generate-preview`, `veo-3.1-fast-generate-preview` | `veo-3.1-generate-preview` | Fast variant same price |
| `--duration` | 4, 6, 8 | 8 | Seconds |
| `--resolution` | 720p, 1080p | 720p | 1080p only with 8s duration |
| `--aspect-ratio` | 16:9, 9:16 | 16:9 | Landscape or portrait |
| `--negative-prompt` | Any text | None | What NOT to include |
| `--no-audio` | Flag | false | Disable audio generation |
| `--output` | Directory path | ./output | Where to save video |
| `--filename` | String | Auto-generated | Output filename |

## Common Mistakes

- **Vague prompts** — "A chef working" vs "Slow tracking shot following a chef plating a dessert"
- **Missing cinematography** — Always start with camera movement/shot type
- **String duration** — API expects integer `8`, not string `"8"`
- **1080p with short duration** — 1080p only works with 8-second videos
- **Wrong image field** — Must be `bytesBase64Encoded`, not `data` or `imageBytes`
- **Forgetting 2-day expiry** — Generated videos deleted from Google servers after 2 days

## Example Prompts

**Cinematic product shot:**
```
Slow dolly shot toward an artisan coffee cup on a wooden table,
steam rising in soft morning light through a window. Shallow depth
of field, warm golden tones, cozy cafe ambiance.
SFX: gentle coffee shop murmur, soft jazz piano in background.
```

**Character animation:**
```
Extreme close-up, shallow depth of field. SILENT SCENE - no dialogue.
A young woman with silver-streaked dark hair, warm closed-mouth smile,
performs a gentle beckoning motion toward the viewer. Tiny magical
sparkles rise from her open palm, drifting upward like stardust.
Hair drifts slightly in a soft breeze. Warm magical atmosphere,
soft diffused lighting.
Audio: soft celestial chimes, gentle twinkle effects. No voices.
```

**First-person journey:**
```
First-person camera perspective. The camera floats weightlessly
through a colorful asteroid field, gently drifting past crystal-studded
rocks catching starlight. A comet with a sparkling rainbow tail swoops
past, leaving trails of stardust. Distant nebulae swirl in cotton-candy
pinks and electric blues. The camera banks smoothly, revealing a
magnificent glowing planet ahead.
Audio: ethereal hum, gentle whooshes, crystalline twinkling.
```
