#!/usr/bin/env bash
set -eo pipefail

# Fallback default values
SPEAKER_NAME="${SPEAKER_NAME:-standalone-speaker}"
VIOX_HOST="${VIOX_HOST:-127.0.0.1}"
SNAPCLIENT_LATENCY="${SNAPCLIENT_LATENCY:-30}"
SOUNDCARD="${SOUNDCARD:-hardware}"

echo "=========================================="
echo "Initializing Standalone Snapclient Host"
echo "  Target Host:       ${VIOX_HOST}"
echo "  Speaker Name:      ${SPEAKER_NAME}"
echo "  ALSA Device:       ${SOUNDCARD}"
echo "  ALSA Card Index:   ${AUDIO_CARD}"
echo "  Target Latency:    ${SNAPCLIENT_LATENCY} ms"
echo "=========================================="

# 1. Pre-flight Hardware Access Check
if [ ! -d /dev/snd ]; then
  echo "WARNING: /dev/snd directory not found!"
  echo "Ensure you pass sound hardware into Docker using '--device /dev/snd' or Compose 'devices:' mapping."
fi

# 2. Graceful Termination Handler
cleanup() {
    echo "Termination signal received. Shutting down Snapclient..."
    if [ -n "$SNAPCLIENT_PID" ]; then
        kill -TERM "$SNAPCLIENT_PID" 2>/dev/null || true
        wait "$SNAPCLIENT_PID" 2>/dev/null || true
    fi
    echo "Snapclient stopped cleanly."
    exit 0
}

trap cleanup SIGINT SIGTERM

# 3. Process Launch Function
start_snapclient() {
    snapclient \
        --player alsa \
        -s "$SOUNDCARD" \
        --hostID "$SPEAKER_NAME" \
        --sampleformat "48000:16:*" \
        --latency "$SNAPCLIENT_LATENCY" \
        --host "$VIOX_HOST" \
        --logsink stdout &

    SNAPCLIENT_PID=$!
}

# Initial Execution
start_snapclient

echo "Snapclient running under PID $SNAPCLIENT_PID. Monitoring health..."

# 4. Continuous Process Health Monitor
while true; do
    if ! kill -0 "$SNAPCLIENT_PID" 2>/dev/null; then
        echo "CRITICAL: Snapclient process died unexpectedly!"
        echo "Attempting to restart Snapclient in 3 seconds..."
        sleep 3
        start_snapclient
    fi

    sleep 5
done
