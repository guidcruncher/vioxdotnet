FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

LABEL org.opencontainers.image.source="https://github.com/guidcruncher/vioxdotnet"
LABEL org.opencontainers.image.title="VioxDotNet Base Image"
LABEL org.opencontainers.image.description="VioxDotNet Base Image contains all dependencies prebundled"
LABEL org.opencontainers.image.licenses="MIT"

WORKDIR /app

RUN DEBIAN_FRONTEND=noninteractive && apt-get update && apt-get install -y --no-install-recommends \
    alsa-utils \
    libasound2-plugins \
    libasound2-plugin-equal \
    caps \
    curl \
    ca-certificates \
    jq \
    tar \
    build-essential \
    cmake \
    git \
    libasound2-dev \
    libpulse-dev \
    libvorbisidec-dev \
    libvorbis-dev \
    libflac-dev \
    libboost-all-dev \
    libavahi-client-dev \
    libexpat1-dev \
    mpd \
    mpc \
    && rm -rf /var/lib/apt/lists/*

# Download and install the latest go-librespot binary for ARM64
RUN GO_LIBRESPOT_URL=$(curl -s https://api.github.com/repos/devgianlu/go-librespot/releases/latest | jq -r '.assets[] | select(.name | startswith("go-librespot")) | select(.name | endswith("linux_arm64.tar.gz")) | .browser_download_url') && \
    curl -sL "$GO_LIBRESPOT_URL" -o go-librespot.tar.gz && \
    tar -xzf go-librespot.tar.gz -C /usr/local/bin go-librespot && \
    chmod +x /usr/local/bin/go-librespot && \
    rm go-librespot.tar.gz

# Download Snapcast release .deb packages or compile from source if no direct arm64 deb release asset is present
RUN SNAPCLIENT_URL=$(curl -s https://api.github.com/repos/Snapcast/snapcast/releases/latest | jq -r '.assets[]? | select(.name != null) | select(.name | contains("snapclient")) | select(.name | contains("arm64")) | select(.name | contains("deb")) | select(.name | contains("pulse") | not) | select(.name | contains("pipewire") | not) | .browser_download_url' | head -n 1) && \
    SNAPSERVER_URL=$(curl -s https://api.github.com/repos/Snapcast/snapcast/releases/latest | jq -r '.assets[]? | select(.name != null) | select(.name | contains("snapserver")) | select(.name | contains("arm64")) | select(.name | contains("deb")) | select(.name | contains("pulse") | not) | select(.name | contains("pipewire") | not) | .browser_download_url' | head -n 1) && \
    if [ -n "$SNAPCLIENT_URL" ] && [ -n "$SNAPSERVER_URL" ]; then \
        curl -sL "$SNAPCLIENT_URL" -o snapclient.deb && \
        curl -sL "$SNAPSERVER_URL" -o snapserver.deb && \
        apt-get update && \
        apt-get install -y --no-install-recommends ./snapclient.deb ./snapserver.deb && \
        rm snapclient.deb snapserver.deb && \
        rm -rf /var/lib/apt/lists/* ; \
    else \
        git clone --recursive https://github.com/Snapcast/snapcast.git /tmp/snapcast && \
        cd /tmp/snapcast && \
        mkdir build && cd build && \
        cmake .. -DBUILD_WITH_PULSE=OFF -DBUILD_WITH_PIPEWIRE=OFF && \
        make -j$(nproc) && \
        make install && \
        rm -rf /tmp/snapcast ; \
    fi

# Configure ALSA sound server to pass audio through the equalizer plugin
RUN printf 'pcm.!default {\n\
    type plug\n\
    slave.pcm "equal"\n\
}\n\
ctl.equal {\n\
    type equal\n\
}\n\
pcm.equal {\n\
    type equal\n\
    slave.pcm "plughw:0,0"\n\
}\n' > /etc/asound.conf

