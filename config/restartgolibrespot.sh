#!/bin/bash
kill $(pgrep -x golibrespot)

go-librespot --config_dir /data/golibrespot/ > /var/log/audio-services/go-librespot.log 2>&1 &
LIBRESPOT_PID=$!
