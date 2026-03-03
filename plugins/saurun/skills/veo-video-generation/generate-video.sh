#!/usr/bin/env bash
set -euo pipefail

# Veo 3.1 Video Generation via Google Generative Language API
# Requires: GEMINI_API_KEY env var, jq, curl

# --- Defaults ---
MODEL="veo-3.1-generate-preview"
DURATION=8
RESOLUTION="720p"
ASPECT_RATIO="16:9"
OUTPUT_DIR="./output"
FILENAME=""
IMAGE_PATH=""
PROMPT=""
NEGATIVE_PROMPT=""
NO_AUDIO=false
POLL_INTERVAL=10
MAX_POLL_ATTEMPTS=36  # 6 minutes

# --- Usage ---
usage() {
  cat <<'EOF'
Usage: generate-video.sh --prompt "..." [options]

Required:
  --prompt, -p        Video generation prompt

Options:
  --image, -i         Input image path (.png/.jpg/.jpeg/.webp) for image-to-video
  --model, -m         Model (default: veo-3.1-generate-preview)
                      Options: veo-3.1-generate-preview, veo-3.1-fast-generate-preview
  --duration, -d      Duration in seconds: 4, 6, or 8 (default: 8)
  --resolution, -r    Resolution: 720p or 1080p (default: 720p, 1080p requires 8s)
  --aspect-ratio, -a  Aspect ratio: 16:9 or 9:16 (default: 16:9)
  --negative-prompt   Text describing what NOT to include
  --no-audio          Disable audio generation
  --output, -o        Output directory (default: ./output)
  --filename, -f      Output filename (default: auto-generated)
  --help, -h          Show this help
EOF
  exit 0
}

# --- Parse args ---
while [[ $# -gt 0 ]]; do
  case $1 in
    --prompt|-p) PROMPT="$2"; shift 2 ;;
    --image|-i) IMAGE_PATH="$2"; shift 2 ;;
    --model|-m) MODEL="$2"; shift 2 ;;
    --duration|-d) DURATION="$2"; shift 2 ;;
    --resolution|-r) RESOLUTION="$2"; shift 2 ;;
    --aspect-ratio|-a) ASPECT_RATIO="$2"; shift 2 ;;
    --negative-prompt) NEGATIVE_PROMPT="$2"; shift 2 ;;
    --no-audio) NO_AUDIO=true; shift ;;
    --output|-o) OUTPUT_DIR="$2"; shift 2 ;;
    --filename|-f) FILENAME="$2"; shift 2 ;;
    --help|-h) usage ;;
    *) echo "Unknown option: $1"; usage ;;
  esac
done

# --- Validate ---
if [[ -z "$PROMPT" ]]; then
  echo "Error: --prompt is required"
  exit 1
fi

if [[ -z "${GEMINI_API_KEY:-}" ]]; then
  echo "Error: GEMINI_API_KEY environment variable not set"
  exit 1
fi

if ! command -v jq &>/dev/null; then
  echo "Error: jq is required (brew install jq)"
  exit 1
fi

if [[ "$RESOLUTION" == "1080p" && "$DURATION" -ne 8 ]]; then
  echo "Warning: 1080p requires 8s duration. Setting duration to 8."
  DURATION=8
fi

# --- Build image payload ---
IMAGE_JSON="null"
if [[ -n "$IMAGE_PATH" ]]; then
  if [[ ! -f "$IMAGE_PATH" ]]; then
    echo "Error: Image file not found: $IMAGE_PATH"
    exit 1
  fi
  EXT="${IMAGE_PATH##*.}"
  case "$EXT" in
    png) MIME="image/png" ;;
    jpg|jpeg) MIME="image/jpeg" ;;
    webp) MIME="image/webp" ;;
    *) echo "Error: Unsupported image format: $EXT (use png/jpg/jpeg/webp)"; exit 1 ;;
  esac
  BASE64_FILE=$(mktemp)
  base64 < "$IMAGE_PATH" | tr -d '\n' > "$BASE64_FILE"
  IMAGE_JSON=$(jq -n --arg mime "$MIME" --rawfile data "$BASE64_FILE" \
    '{"mimeType": $mime, "bytesBase64Encoded": $data}')
  rm -f "$BASE64_FILE"
fi

# --- Build request body ---
PARAMS=$(jq -n \
  --arg ar "$ASPECT_RATIO" \
  --argjson dur "$DURATION" \
  --arg res "$RESOLUTION" \
  --arg neg "$NEGATIVE_PROMPT" \
  --argjson noaudio "$NO_AUDIO" \
  '{
    aspectRatio: $ar,
    durationSeconds: $dur,
    resolution: $res
  }
  + (if $neg != "" then {negativePrompt: $neg} else {} end)
  + (if $noaudio then {generateAudio: false} else {} end)')

if [[ "$IMAGE_JSON" == "null" ]]; then
  BODY=$(jq -n --arg prompt "$PROMPT" --argjson params "$PARAMS" \
    '{instances: [{prompt: $prompt}], parameters: $params}')
else
  BODY=$(jq -n --arg prompt "$PROMPT" --argjson img "$IMAGE_JSON" --argjson params "$PARAMS" \
    '{instances: [{prompt: $prompt, image: $img}], parameters: $params}')
fi

# --- Submit request ---
ENDPOINT="https://generativelanguage.googleapis.com/v1beta/models/${MODEL}:predictLongRunning"

echo "Submitting video generation request..."
echo "  Model: $MODEL"
echo "  Duration: ${DURATION}s | Resolution: $RESOLUTION | Aspect: $ASPECT_RATIO"
[[ -n "$IMAGE_PATH" ]] && echo "  Image: $IMAGE_PATH"
echo ""

RESPONSE=$(curl -s -w "\n%{http_code}" \
  -X POST "$ENDPOINT" \
  -H "x-goog-api-key: $GEMINI_API_KEY" \
  -H "Content-Type: application/json" \
  -d "$BODY")

HTTP_CODE=$(echo "$RESPONSE" | tail -1)
RESPONSE_BODY=$(echo "$RESPONSE" | sed '$d')

if [[ "$HTTP_CODE" -ne 200 ]]; then
  echo "Error: API returned HTTP $HTTP_CODE"
  echo "$RESPONSE_BODY" | jq . 2>/dev/null || echo "$RESPONSE_BODY"
  exit 1
fi

OPERATION_NAME=$(echo "$RESPONSE_BODY" | jq -r '.name // empty')
if [[ -z "$OPERATION_NAME" ]]; then
  echo "Error: No operation name in response"
  echo "$RESPONSE_BODY" | jq .
  exit 1
fi

echo "Operation started: $OPERATION_NAME"
echo "Polling every ${POLL_INTERVAL}s (max ${MAX_POLL_ATTEMPTS} attempts)..."
echo ""

# --- Poll for completion ---
POLL_URL="https://generativelanguage.googleapis.com/v1beta/${OPERATION_NAME}"
ATTEMPT=0

while [[ $ATTEMPT -lt $MAX_POLL_ATTEMPTS ]]; do
  sleep "$POLL_INTERVAL"
  ATTEMPT=$((ATTEMPT + 1))

  POLL_RESPONSE=$(curl -s \
    "$POLL_URL" \
    -H "x-goog-api-key: $GEMINI_API_KEY")

  DONE=$(echo "$POLL_RESPONSE" | jq -r '.done // false')

  if [[ "$DONE" == "true" ]]; then
    # Check for error
    ERROR_MSG=$(echo "$POLL_RESPONSE" | jq -r '.error.message // empty')
    if [[ -n "$ERROR_MSG" ]]; then
      echo "Error: Video generation failed: $ERROR_MSG"
      exit 1
    fi

    # Extract video URI
    VIDEO_URI=$(echo "$POLL_RESPONSE" | jq -r '.response.generateVideoResponse.generatedSamples[0].video.uri // empty')
    if [[ -z "$VIDEO_URI" ]]; then
      echo "Error: No video URI in response"
      echo "$POLL_RESPONSE" | jq .
      exit 1
    fi

    echo "Video ready! Downloading..."

    # Download
    mkdir -p "$OUTPUT_DIR"
    if [[ -z "$FILENAME" ]]; then
      FILENAME="video-$(date +%Y%m%d-%H%M%S).mp4"
    fi
    OUTPUT_PATH="${OUTPUT_DIR}/${FILENAME}"

    curl -s -o "$OUTPUT_PATH" \
      "$VIDEO_URI" \
      -H "x-goog-api-key: $GEMINI_API_KEY"

    FILE_SIZE=$(wc -c < "$OUTPUT_PATH" | tr -d ' ')
    FILE_SIZE_MB=$(echo "scale=1; $FILE_SIZE / 1048576" | bc)

    echo ""
    echo "Success! Video saved to: $OUTPUT_PATH"
    echo "File size: ${FILE_SIZE_MB} MB"
    echo ""
    echo "Note: Video stored on Google servers for 2 days only."
    exit 0
  fi

  echo "[$(date +%H:%M:%S)] Generating... (attempt $ATTEMPT/$MAX_POLL_ATTEMPTS)"
done

echo "Error: Timeout after $((MAX_POLL_ATTEMPTS * POLL_INTERVAL)) seconds"
exit 1
