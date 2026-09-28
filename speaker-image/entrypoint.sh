#!/bin/bash

set -e

snapclient --player alsa -s "hw:CARD=AUDIO,DEV=0" \
    --hostID "$SPEAKER_NAME" \
    --sampleformat "48000:16:*" \
    --logsink stdout \
    "$VIOX_HOST":

