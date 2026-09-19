#!/bin/bash
set -e

mkdir -p /audio /tmp /run/mpd /var/lib/mpd/ /music/cache /data /data/cache /data/playlists /data/golibrespot
cp /etc/golibrespot/config.yml /data/golibrespot/config.yml

rm -rf /data/auth_token.json /audio/output /tmp/mpd_socket.sock

mkfifo /audio/output
mkfifo /tmp/mpd_socket.sock

chmod 666 /tmp/mpd_socket.sock
chmod 666 /audio/output
chmod 777 -R /music

# Create log directory if it does not exist
mkdir -p /var/log/audio-services

# Trap termination signals to gracefully stop background processes
cleanup() {
    echo "Termination signal received. Shutting down background processes..."
    mpd --kill
    kill -TERM "$SNAPCLIENT_PID" "$SNAPSERVER_PID" "$LIBRESPOT_PID" "$DOTNET_PID" 2>/dev/null || true
    wait "$SNAPCLIENT_PID" "$SNAPSERVER_PID" "$LIBRESPOT_PID" "$DOTNET_PID" 2>/dev/null || true
    echo "Services stopped cleanly."
    exit 0
}

trap cleanup SIGTERM SIGINT

# 1. Start Snapserver in background
echo "Starting Snapserver..."
snapserver > /var/log/audio-services/snapserver.log 2>&1 &
SNAPSERVER_PID=$!

# 2. Start go-librespot in background
echo "Starting go-librespot..."
go-librespot --config_dir /data/golibrespot/ > /var/log/audio-services/go-librespot.log 2>&1 &
LIBRESPOT_PID=$!

# 3. Start Snapclient
echo "Starting Snapclient..."
snapclient --player alsa -s "hw:CARD=AUDIO,DEV=0" \
    --hostID "viox-net" \
    --sampleformat "44100:16:*" \
    --logsink stdout \
    tcp://127.0.0.1 > /var/log/audio-services/snapclient.log 2>&1 &
SNAPCLIENT_PID=$!

# 4. Start MPD in background
echo "Starting MPD..."
mpd
mpc update

# 5. Start .NET 10 Web API in background
echo "Starting .NET 10 Web API..."
dotnet Viox.Server.dll &
DOTNET_PID=$!

# Process health check loop
while true; do
    if ! kill -0 "$SNAPSERVER_PID" 2>/dev/null; then
        echo "ERROR: Snapserver process died unexpectedly. Check /var/log/audio-services/snapserver.log"
        cleanup
        exit 1
    fi

    if ! kill -0 "$SNAPCLIENT_PID" 2>/dev/null; then
        echo "ERROR: Snapclient process died unexpectedly. Check /var/log/audio-services/snapclient.log"
        cleanup
        exit 1
    fi

    if ! kill -0 "$DOTNET_PID" 2>/dev/null; then
        echo "ERROR: .NET Web API process died unexpectedly."
        cleanup
        exit 1
    fi

    sleep 2
done
