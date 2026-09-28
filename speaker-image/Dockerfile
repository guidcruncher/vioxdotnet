FROM guidcruncher/vioxdotnet-base AS final

LABEL org.opencontainers.image.source="https://github.com/guidcruncher/vioxdotnet"
LABEL org.opencontainers.image.title="VioxDotNet Speaker Image"
LABEL org.opencontainers.image.description="VioxDotNet Speaker Image contains a Snapcast Client for use as a remote speaker driver."
LABEL org.opencontainers.image.licenses="MIT"
WORKDIR /app

COPY ./asound.conf /etc/asound.conf
COPY ./entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/*.sh

EXPOSE 1704/tcp
EXPOSE 1705/tcp
EXPOSE 2000/tcp
EXPOSE 5353/udp

ENV SPEAKER_NAME=""
ENV VIOX_HOST=""

ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]
