# VIOXDotNet Project Guide

VIOXDotNet is a .NET 10 music server with a Vue-based control interface. It brings
media catalogs, local libraries, playback engines, a playback queue, audio
processing, and Snapcast multi-room control together behind one HTTP API.

For endpoint details, request and response schemas, and example payloads, see
[the API reference](api-docs.md). The repository's sample server configuration
is [appsettings-sample.json](appsettings-sample.json).

## Architecture

```text
AudioHub (Vue)
     |
     | HTTP API and server-sent events
     v
Viox.Server (ASP.NET Core)
     | controllers
     +-- core services: media resolution, queue, playlists, favorites, events
     +-- media-source modules: Spotify, TuneIn, Podverse, radio, files, ...
     +-- player control surface
            +-- Librespot adapter
            +-- MPD adapter
     +-- Snapcast client and EPG services
```

`Viox.Server` is the host and composition root. `Program.cs` registers core and
server services, configures the HTTP pipeline, maps controllers, and serves the
built AudioHub application when its web assets are available. Controllers expose
HTTP endpoints and delegate application work to services; media providers and
player implementations are registered behind shared interfaces.

The unified media-player control surface resolves a media URI through the
registered media sources, obtains its playback URL, and routes playback to an
adapter that can handle it. The included adapters control Librespot and MPD.
Playback queue events can request playback or stop operations, and media events
feed back into queue behavior. Snapcast is a separate mixer/control integration
for connected clients and groups.

Media items use structured `MediaUri` values and their string form,
`source:type:id[:secondaryId]`, to identify content across providers. The shared
media-source interface supports metadata resolution, child-item lookup,
search, library reads, and playback URL resolution.

## Repository layout

| Path | Responsibility |
| --- | --- |
| `src/Viox.Server` | ASP.NET Core host, API controllers, player adapters, queue, and web hosting |
| `src/Viox.Core` | Shared media models, service abstractions, persistence, events, playlists, and plugin loading |
| `src/Viox.Client.*` | Provider and device integrations for local files, Spotify, TuneIn, Podverse, Radio Browser, Librespot, and MPD |
| `src/Viox.Snapcast` | Snapcast JSON-RPC client and connection services |
| `src/Viox.Epg` | Electronic programme guide import, storage, and query services |
| `src/Plugins` | Additional plugin projects, including the YouTube integration |
| `src/AudioHub` | Vue 3 and TypeScript browser application |
| `tests/Viox.Tests.Core` | xUnit tests for shared services and selected integrations |
| `docs` | API reference, configuration sample, compose example, and project documentation |
| `config` and `entrypoint.sh` | Container audio-service configuration and startup |

The solution is `viox.sln` (also represented by `viox.slnx`). The main projects
target `net10.0`; AudioHub is built with Node.js and Vite.

## Media sources and plugins

The core `IMediaSource` abstraction gives the application a consistent way to
search and resolve media regardless of its provider. Provider modules register
their source and metadata converters with dependency injection. Core discovers
modules in referenced assemblies at startup. The built-in integrations include:

- **Local files and playlists:** file scanning and M3U playlist parsing.
- **Spotify:** catalog and library access, OAuth authentication, and metadata
  conversion.
- **TuneIn:** radio browsing, search, and station resolution.
- **Podverse:** podcast and episode discovery and resolution.
- **Radio Browser:** station directory access.
- **Librespot:** player integration and Spotify Connect event handling.
- **MPD:** Music Player Daemon client and playback events.
- **YouTube:** an additional plugin project under `src/Plugins`.

Dynamic plugin assemblies can also be loaded from the directory configured by
`PluginLoader:PluginFolderPath` (default `/plugins`) using
`PluginLoader:SearchPattern` (default `*.dll`). A plugin implements
`IPluginModule` and registers its services in `ConfigureServices`. Dynamic
plugins must be built and deployed with their required dependencies.

## HTTP API and browser application

Controllers use the `/api/v1` route prefix. The API includes media search and
library access, unified playback and queue control, favorites and playlists,
configuration and equalizer controls, server events, Spotify authentication,
Snapcast operations, and EPG-backed services. The full route index is in
[api-docs.md](api-docs.md).

The server generates an OpenAPI document at `/openapi/v1.json` and serves the
interactive Scalar API reference at `/docs/v1`.

AudioHub is a Vue single-page application. Its routes are declared in
`src/AudioHub/src/router/index.ts`; it calls the server API through the
`useApiTransport` composable and uses server-sent events for live updates. For
local frontend development, `VITE_API_BASE_URL` selects the API origin; if it
is unset, requests target the frontend's current origin.

The server serves static AudioHub files from `/app/wwwroot` unless
`DISABLE_AUDIOHUB_WEBUI=true`. The Dockerfile builds AudioHub and copies the
compiled files into that location. A local Vite development server is separate
from this container static-file hosting path.

## Configuration and persisted data

ASP.NET Core configuration is bound to typed options throughout the application.
Use [appsettings-sample.json](appsettings-sample.json) as a starting point; it
documents settings for integrations and services, including `Spotify`,
`SpotifyAuth`, `Mpd`, `Snapcast`, `Podverse`, `TuneIn`, `RadioBrowser`,
`MediaSearch`, `MemoryCacheService`, and `StreamingOptions`.

Set secrets such as Spotify credentials and provider authorization tokens in
the deployment environment or a local configuration file that is not committed
to source control. ASP.NET Core environment variables can override nested JSON
settings using double underscores, for example
`SpotifyAuth__ClientId`.

The container configuration stores persistent application data under `/data`,
including favorites and client options. Playlist repositories, Spotify auth
tokens, caches, music files, and plugin assemblies use their configured paths
or container mounts. The container entrypoint creates runtime directories and
starts Snapserver, go-librespot, Snapclient, MPD, and the .NET API together.

## Build, run, and test

Prerequisites depend on the workflow:

- .NET SDK 10 for the server and tests.
- Node.js and npm for the AudioHub development server and frontend build.
- Docker and Docker Compose for the complete containerized audio stack.
- Linux audio devices and the configured audio services for actual playback.

Restore and build the .NET solution:

```sh
dotnet restore viox.sln
dotnet build src/Viox.Server/Viox.Server.csproj -c Release
```

Build and run the frontend separately:

```sh
cd src/AudioHub
npm ci
npm run build
npm run dev
```

When using Vite, configure `VITE_API_BASE_URL` to point at the running API
origin. To run the server locally, use `dotnet run --project
src/Viox.Server/Viox.Server.csproj`; set `ASPNETCORE_ENVIRONMENT=Development` to
enable the development CORS policy when the frontend and API use different
origins.

Run the tests with:

```sh
dotnet test viox.sln
```

Coverlet is available through the test project collector:

```sh
dotnet test tests/Viox.Tests.Core/Viox.Tests.Core.csproj \
  --collect:"XPlat Code Coverage"
```

The repository `Makefile` provides convenience targets such as `make test`,
`make dotnet-build`, `make client`, `make client-build`, `make dev`, and `make
up`. The root Compose files build a Linux ARM64 image and configure host
networking, sound-device access, and persistent runtime mounts. Review
`docker-compose.yml`, `docker-compose.dev.yml`, and the configuration files
before running the full stack; local audio hardware and runtime configuration
are deployment-specific.
