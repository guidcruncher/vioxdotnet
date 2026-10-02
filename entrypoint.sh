#!/usr/bin/env bash
set -eo pipefail

# 0. Pre-flight Hardware Access Check
if [ ! -d /dev/snd ]; then
  echo "WARNING: /dev/snd directory not found!"
  echo "Ensure you pass sound hardware into Docker using '--device /dev/snd' or Compose 'devices:' mapping."
fi

# 1. Directory Structure Setup
mkdir -p /tmp /run/mpd /var/lib/mpd /music/cache /data /data/cache /data/playlists /data/golibrespot /data/snapserver /var/log/audio-services

# Copy default golibrespot config ONLY if a persistent config does not already exist
if [ ! -f /data/golibrespot/config.yml ] && [ -f /etc/golibrespot/config.yml ]; then
    echo "Initializing default Golibrespot configuration..."
    envsubst < /etc/golibrespot/config.yml > /data/golibrespot/config.yml
fi

# 2. FIFO Pipe Cleanup & Initialization
rm -f /tmp/snapfifo /tmp/mpd_socket.sock

mkfifo /tmp/snapfifo
mkfifo /tmp/mpd_socket.sock

chmod 666 /tmp/snapfifo /tmp/mpd_socket.sock

# 3. Graceful Termination Handler
cleanup() {
    echo "Termination signal received. Gracefully shutting down audio services and .NET server..."

    # Gracefully stop MPD
    mpd --kill 2>/dev/null ||utrue

    # Terminate remaining background jobs
    if [ -n "$SNAPSERVER_PID" ] || [ -n "$LIBRESPOT_PID" ] || [ -n "$SNAPCLIENT_PID" ] || [ -n "$DOTNET_PID" ]; then
        kill -TERM "$SNAPSERVER_PID" "$LIBRESPOT_PID" "$SNAPCLIENT_PID" "$DOTNET_PID" 2>/dev/null || true
        wait "$SNAPSERVER_PID" "$LIBRESPOT_PID" "$SNAPCLIENT_PID" "$DOTNET_PID" 2>/dev/null || true
    fi

    echo "All processes stopped cleanly."
    exit 0
}

trap cleanup SIGTERM SIGINT

# 4. Start Services

echo "Starting Snapserver..."
envsubst < /etc/snapserver.conf.template > /etc/snapserver.conf
snapserver --config /etc/snapserver.conf &
SNAPSERVER_PID=$!

echo "Starting go-librespot..."
go-librespot --config_dir /data/golibrespot/ &
LIBRESPOT_PID=$!

echo "Starting Snapclient (Targeting ALSA 'hardware' device)..."
# Critical: Use -s hardware to prevent audio loopback through alsaequal/snapfifo
snapclient --player alsa \
    -s "hardware" \
    --hostID "${DEVICE_NAME}" \
    --sampleformat "44100:16:*" \
    --latency 30 \
    tcp://127.0.0.1:1704 &
SNAPCLIENT_PID=$!

echo "Starting MPD..."
# Run MPD in no-daemon mode so we capture its PID cleanly for health tracking
envsubst < /etc/mpd.conf.template > /etc/mpd.conf
mpd --no-daemon &
MPD_PID=$!

# Wait briefly for MPD socket initialization before triggering mpc
sleep 1
mpc volume $INITIAL_VOLUME 2>/dev/null || true
mpc update 2>/dev/null || true

echo "Starting .NET 10 Web API..."
dotnet Viox.Server.dll &
DOTNET_PID=$!

# 5. Process Health Monitor Loop
echo "All audio services started. Monitoring process health..."

while true; do
    if ! kill -0 "$SNAPSERVER_PID" 2>/dev/null; then
        echo "CRITICAL: Snapserver process died unexpectedly."
        cleanup
    fi

    if ! kill -0 "$LIBRESPOT_PID" 2>/dev/null; then
        echo "CRITICAL: go-librespot process died unexpectedly."
        cleanup
    fi

    if ! kill -0 "$SNAPCLIENT_PID" 2>/dev/null; then
        echo "CRITICAL: Snapclient process died unexpectedly."
        cleanup
    fi

    if ! kill -0 "$MPD_PID" 2>/dev/null; then
        echo "CRITICAL: MPD process died unexpectedly."
        cleanup
    fi

    if ! kill -0 "$DOTNET_PID" 2>/dev/null; then
        echo "CRITICAL: .NET Web API process died unexpectedly."
        cleanup
    fi

    sleep 3
done
