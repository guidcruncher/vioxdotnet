# Define build argument for project name with default fallback
ARG APP_PROJECT=Viox.Server/Viox.Server.csproj

# Stage 1: Build Node.js Vue TypeScript Frontend
FROM node:20-alpine AS node-build
ENV VITE_API_BASE_URL=
WORKDIR /src/AudioHub

# Copy package manifests and restore dependencies to leverage layer caching
COPY src/AudioHub/package*.json ./
RUN npm ci

# Copy frontend source files and compile Vite build to ./src/AudioHub/wwwroot
COPY src/AudioHub ./
RUN npm run build

# Stage 2: Build .NET 10 Web API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG APP_PROJECT
WORKDIR /src

# Copy project source files
COPY ./src .

# Restore dependencies and publish application
RUN dotnet restore "./${APP_PROJECT}"
RUN dotnet publish "./${APP_PROJECT}" -c Release -o /app/publish /p:UseAppHost=false

# Copy compiled frontend assets from node-build stage into wwwroot directory
COPY --from=node-build /src/AudioHub/wwwroot /app/publish/wwwroot

# Stage 3: Runtime Environment
FROM guidcruncher/vioxdotnet-base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV AudioSettings__DefaultOutputDevice=equal
ENV AudioSettings__EnableEqualizer=true
ENV Logging__LogLevel__Default=Information
ENV Logging__LogLevel__Microsoft.AspNetCore=Warning

# Config
RUN mkdir -p /etc/golibrespot /music /data/golibrespot
COPY ./config/snapserver.conf /etc/snapserver.conf
COPY ./config/config.yml /etc/golibrespot/config.yml
COPY ./config/config.yml /etc/golibrespot/config-template.yml
COPY ./config/mpd.conf /etc/mpd.conf
RUN chmod -R 755 /music

# Copy published .NET application
COPY --from=build /app/publish .
RUN rm /app/appsettings*.*

# Copy and set entrypoint script permissions
COPY ./config/restartgolibrespot.sh /app/restartgolibrespot.sh
COPY ./config/mpddebug.sh /app/mpddebug.sh
COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/*.sh

EXPOSE 1704/tcp
EXPOSE 1705/tcp
EXPOSE 2000/tcp
EXPOSE 5353/udp

ENTRYPOINT ["/app/entrypoint.sh"]
