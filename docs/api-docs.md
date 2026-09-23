# Viox.Net API

> **The Viox API** · version `v1.0` · OpenAPI 3.1.1

Viox.Net is a unified media control and discovery API. It fronts local playback backends (generic media player, MPD, Librespot) and multi-room mixing (Snapcast), plus catalog and streaming sources (Spotify, TuneIn, Radio Browser, Podverse, and a generic library).

---

## Base URL

```
http://192.168.1.202:8080/
```

All paths below are relative to this origin. Request and response bodies use `application/json` unless noted.

## Contents

1. [Overview](#overview)
2. [Conventions](#conventions)
3. [Authentication](#authentication)
4. [Library](#library)
5. [Librespot](#librespot)
6. [Media Player](#media-player)
7. [Media Search](#media-search)
8. [MPD](#mpd)
9. [Podverse](#podverse)
10. [Radio Browser](#radio-browser)
11. [Snapcast](#snapcast)
12. [Spotify Auth](#spotify-auth)
13. [Spotify](#spotify)
14. [TuneIn](#tunein)
15. [Common schemas](#common-schemas)
16. [Error model](#error-model)
17. [Endpoint index](#endpoint-index)

---

## Overview

The API is organised into tagged groups:

| Group | Role |
|---|---|
| **Library** | List items from a named local/remote media source. |
| **Media Player** | Backend-agnostic playback control (play, pause, seek, volume, now playing). |
| **Librespot** | Spotify Connect-style control of a Librespot player. |
| **MPD** | Direct Music Player Daemon session and playlist control. |
| **Snapcast** | Multi-room mixer: clients, groups, volume, latency, streams. |
| **Media Search** | Keyword search across all sources or a single source key. |
| **Spotify / Spotify Auth** | OAuth login plus catalog and library lookups. |
| **TuneIn** | Browse, search, describe, and tune internet radio. |
| **Radio Browser** | Community radio-station directory and click tracking. |
| **Podverse** | Podcast and episode metadata. |

Typical client flow:

1. Discover or search content (`/api/v1/media/search`, Spotify, TuneIn, Radio Browser, Podverse, Library).
2. Start playback via the unified player (`POST /api/v1/media-player/play` with a URI) or a backend-specific player (Librespot / MPD).
3. Observe state (`GET /api/v1/media-player/current`) and adjust volume / groups via Snapcast if multi-room output is in use.

---

## Conventions

- **URI scheme.** Playable items are identified by a string `uri` and optionally a structured `MediaUri` (`source`, `type`, `id`, `secondaryId`). Pass the string form to playback endpoints.
- **Pagination.** Search endpoints accept `pageNumber` (default `1`) and `limit` (default `20`) and return a `PagedListOfMediaMetaData`.
- **Numeric dual types.** Several integer and number fields are typed as `integer | string` (or `number | string`) in the spec. Send JSON numbers unless a formatted string is required (see seek).
- **Seek formats.**
  - Media Player `SeekRequest.position` is a duration string matching `[-][d.]HH:MM:SS[.fffffff]`.
  - Librespot `ApiSeekRequest.position` is an `int64` (milliseconds implied by the librespot protocol); `relative` selects relative vs absolute seek.
- **Volume.** `volumePercent` is an integer `0–100`. Snapcast and the media player both use `SetVolumeRequest` / `VolumeState`.
- **Errors.** Failed requests return RFC 7807-style `ProblemDetails` or `ValidationProblemDetails` (search).

---

## Authentication

Most local-control endpoints have no auth scheme declared in this spec. Spotify catalog and `/me` routes depend on a prior OAuth session:

1. `GET /api/v1/auth/login` — starts the Spotify OAuth redirect (`302 Found`).
2. `GET /api/v1/auth/callback` — handles `code`, `state`, and `error` query params after Spotify redirects back.
3. `POST /api/v1/auth/refresh` — body is a refresh-token **string**; returns a refreshed session.

Store tokens server-side; this API does not document cookie vs header transport.

---

## Library

Enumerate items from a configured media source by source key.

### `GET /api/v1/library/{source}`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `source` | path | `string` | yes | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | array of `MediaMetaData` |
| `400` | Bad Request | `ProblemDetails` |
| `404` | Not Found | `ProblemDetails` |

---

## Librespot

Control a Librespot (Spotify Connect) player: play a URI, transport, shuffle, and repeat.

### `GET /api/v1/players/librespot/state`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |

### `GET /api/v1/players/librespot/events`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |

### `POST /api/v1/players/librespot/play`

**Request body**

Required: **yes**. Content type: `application/json`.

Schema: [`ApiPlayRequest`](#apiplayrequest)

| Field | Type | Constraint |
|---|---|---|
| `uri` | `string` | **required** |
| `skip_to_uri` | `string | null` | optional |
| `paused` | `boolean | null` | optional |
| `position` | `integer | string | null (int64)` | optional |

Example:

```json
{
  "uri": "spotify:track:4uLU6hMCjMI75M1A2tKUQC",
  "skip_to_uri": "string",
  "paused": false,
  "position": 0
}
```

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |

### `POST /api/v1/players/librespot/pause`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |

### `POST /api/v1/players/librespot/resume`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |

### `POST /api/v1/players/librespot/seek`

**Request body**

Required: **yes**. Content type: `application/json`.

Schema: [`ApiSeekRequest`](#apiseekrequest)

| Field | Type | Constraint |
|---|---|---|
| `position` | `integer | string (int64)` | **required** |
| `relative` | `boolean` | optional |

Example:

```json
{
  "position": 0,
  "relative": false
}
```

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |

### `POST /api/v1/players/librespot/shuffle`

**Request body**

Required: **yes**. Content type: `application/json`.

Schema: [`ApiShuffleContextRequest`](#apishufflecontextrequest)

| Field | Type | Constraint |
|---|---|---|
| `shuffle_context` | `boolean` | **required** |

Example:

```json
{
  "shuffle_context": false
}
```

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |

### `POST /api/v1/players/librespot/repeat-context`

**Request body**

Required: **yes**. Content type: `application/json`.

Schema: [`ApiRepeatContextRequest`](#apirepeatcontextrequest)

| Field | Type | Constraint |
|---|---|---|
| `repeat_context` | `boolean` | **required** |

Example:

```json
{
  "repeat_context": false
}
```

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |

### `POST /api/v1/players/librespot/repeat-track`

**Request body**

Required: **yes**. Content type: `application/json`.

Schema: [`ApiRepeatTrackRequest`](#apirepeattrackrequest)

| Field | Type | Constraint |
|---|---|---|
| `repeat_track` | `boolean` | **required** |

Example:

```json
{
  "repeat_track": false
}
```

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |

---

## Media Player

Unified playback façade over the active backend. Prefer these endpoints from UI clients unless you need backend-specific behaviour.

### `POST /api/v1/media-player/play`

**Request body**

Required: **yes**. Content type: `application/json`.

Schema: [`PlayRequest`](#playrequest)

| Field | Type | Constraint |
|---|---|---|
| `uri` | `string` | **required** |

Example:

```json
{
  "uri": "spotify:track:4uLU6hMCjMI75M1A2tKUQC"
}
```

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |
| `400` | Bad Request | `ProblemDetails` |

### `POST /api/v1/media-player/pause`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |

### `POST /api/v1/media-player/resume`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |

### `POST /api/v1/media-player/stop`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |

### `POST /api/v1/media-player/seek`

**Request body**

Required: **yes**. Content type: `application/json`.

Schema: [`SeekRequest`](#seekrequest)

| Field | Type | Constraint |
|---|---|---|
| `position` | `string` | **required** |

Example:

```json
{
  "position": "00:01:30"
}
```

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |

### `POST /api/v1/media-player/next`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |

### `POST /api/v1/media-player/previous`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |

### `PUT /api/v1/media-player/volume`

**Request body**

Required: **yes**. Content type: `application/json`.

Schema: [`SetVolumeRequest`](#setvolumerequest)

| Field | Type | Constraint |
|---|---|---|
| `volumePercent` | `integer | string (int32)` | **required** (min 0, max 100) |
| `muted` | `boolean` | optional |

Example:

```json
{
  "volumePercent": 50,
  "muted": false
}
```

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | map of `VolumeState` |
| `400` | Bad Request | `ProblemDetails` |

### `GET /api/v1/media-player/volume`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |
| `404` | Not Found | `ProblemDetails` |

### `GET /api/v1/media-player/active`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |

### `GET /api/v1/media-player/current`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `PlaybackState` |

---

## Media Search

Keyword search across every registered source, or pinned to one `sourceKey`.

### `GET /api/v1/media/search`

**Search all media sources**

Executes a keyword query concurrently across every registered media source and returns aggregated results.

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `query` | query | `string` | yes | — | — |
| `pageNumber` | query | `integer | string (int32)` | no | `1` | — |
| `limit` | query | `integer | string (int32)` | no | `20` | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | map of `PagedListOfMediaMetaData` |
| `400` | Bad Request | `ValidationProblemDetails` |
| `500` | Internal Server Error | `ProblemDetails` |

### `GET /api/v1/media/search/{sourceKey}`

**Search a specific media source**

Executes a keyword search query against a single specified media source identified by its source key.

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `sourceKey` | path | `string` | yes | — | — |
| `query` | query | `string` | yes | — | — |
| `pageNumber` | query | `integer | string (int32)` | no | `1` | — |
| `limit` | query | `integer | string (int32)` | no | `20` | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `PagedListOfMediaMetaData` |
| `400` | Bad Request | `ValidationProblemDetails` |
| `404` | Not Found | `ProblemDetails` |
| `500` | Internal Server Error | `ProblemDetails` |

---

## MPD

Session control for Music Player Daemon: connect, transport, play a file/URL, raw commands, and playlist clear.

### `POST /api/v1/mpd/connect`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |
| `500` | Internal Server Error | `object` (unspecified) |

### `POST /api/v1/mpd/disconnect`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |

### `POST /api/v1/mpd/play`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |
| `500` | Internal Server Error | `object` (unspecified) |

### `POST /api/v1/mpd/play-file`

**Request body**

Required: **yes**. Content type: `application/json`.

Schema: [`MpdPlayRequest`](#mpdplayrequest)

| Field | Type | Constraint |
|---|---|---|
| `fileOrUrl` | `string` | **required** |

Example:

```json
{
  "fileOrUrl": "/music/album/track.flac"
}
```

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |
| `400` | Bad Request | `ProblemDetails` |
| `500` | Internal Server Error | `object` (unspecified) |

### `POST /api/v1/mpd/pause`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |
| `500` | Internal Server Error | `object` (unspecified) |

### `POST /api/v1/mpd/stop`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |
| `500` | Internal Server Error | `object` (unspecified) |

### `POST /api/v1/mpd/next`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |
| `500` | Internal Server Error | `object` (unspecified) |

### `POST /api/v1/mpd/previous`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |
| `500` | Internal Server Error | `object` (unspecified) |

### `DELETE /api/v1/mpd/playlist`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |
| `500` | Internal Server Error | `object` (unspecified) |

### `POST /api/v1/mpd/command`

**Request body**

Required: **yes**. Content type: `application/json`.

Schema: [`MpdCommandRequest`](#mpdcommandrequest)

| Field | Type | Constraint |
|---|---|---|
| `command` | `string` | **required** |

Example:

```json
{
  "command": "status"
}
```

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |
| `400` | Bad Request | `ProblemDetails` |
| `500` | Internal Server Error | `object` (unspecified) |

---

## Podverse

Podcast and episode metadata from Podverse.

### `GET /api/v1/media/podverse/podcast/{podcastId}/episodes`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `podcastId` | path | `string` | yes | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | array of `MediaMetaData` |
| `400` | Bad Request | `ProblemDetails` |
| `404` | Not Found | `ProblemDetails` |

### `GET /api/v1/media/podverse/podcasts/{podcastId}`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `podcastId` | path | `string` | yes | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `MediaMetaData` |
| `400` | Bad Request | `ProblemDetails` |
| `404` | Not Found | `ProblemDetails` |

### `GET /api/v1/media/podverse/episodes/{episodeId}`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `episodeId` | path | `string` | yes | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `MediaMetaData` |
| `400` | Bad Request | `ProblemDetails` |
| `404` | Not Found | `ProblemDetails` |

---

## Radio Browser

Community radio directory: stats, servers, station lookup, ranked lists, and click reporting.

### `GET /api/v1/media/radiobrowser/stats`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `ServerStats` |

### `GET /api/v1/media/radiobrowser/servers`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | array of `ApiServer` |

### `GET /api/v1/media/radiobrowser/stations/{stationUuid}`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `stationUuid` | path | `string` | yes | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `MediaMetaData` |
| `404` | Not Found | `ProblemDetails` |

### `GET /api/v1/media/radiobrowser/stations/top-clicked`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `Order` | query | `StationOrder` | no | — | — |
| `Reverse` | query | `boolean` | no | — | — |
| `Offset` | query | `integer | string (int32)` | no | — | — |
| `Limit` | query | `integer | string (int32)` | no | — | — |
| `HideBroken` | query | `boolean` | no | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | array of `MediaMetaData` |

### `GET /api/v1/media/radiobrowser/stations/top-voted`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `Order` | query | `StationOrder` | no | — | — |
| `Reverse` | query | `boolean` | no | — | — |
| `Offset` | query | `integer | string (int32)` | no | — | — |
| `Limit` | query | `integer | string (int32)` | no | — | — |
| `HideBroken` | query | `boolean` | no | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | array of `MediaMetaData` |

### `GET /api/v1/media/radiobrowser/stations/recently-clicked`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `Order` | query | `StationOrder` | no | — | — |
| `Reverse` | query | `boolean` | no | — | — |
| `Offset` | query | `integer | string (int32)` | no | — | — |
| `Limit` | query | `integer | string (int32)` | no | — | — |
| `HideBroken` | query | `boolean` | no | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | array of `MediaMetaData` |

### `POST /api/v1/media/radiobrowser/stations/{stationUuid}/click`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `stationUuid` | path | `string` | yes | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `ClickResult` |

---

## Snapcast

Multi-room audio mixer. Connect to the Snapserver, inspect clients/groups/streams, and set volume, mute, latency, names, and group membership.

### `POST /api/v1/mixer/snapcast/connect`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |
| `500` | Internal Server Error | `object` (unspecified) |

### `POST /api/v1/mixer/snapcast/disconnect`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |
| `500` | Internal Server Error | `object` (unspecified) |

### `GET /api/v1/mixer/snapcast/rpc-version`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `RpcVersion` |

### `GET /api/v1/mixer/snapcast/status`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `SnapServer` |

### `GET /api/v1/mixer/snapcast/clients`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | array of `SnapClient` |

### `PUT /api/v1/mixer/snapcast/clients/{clientId}/volume`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `clientId` | path | `string` | yes | — | — |

**Request body**

Required: **yes**. Content type: `application/json`.

Schema: [`SetVolumeRequest`](#setvolumerequest)

| Field | Type | Constraint |
|---|---|---|
| `volumePercent` | `integer | string (int32)` | **required** (min 0, max 100) |
| `muted` | `boolean` | optional |

Example:

```json
{
  "volumePercent": 50,
  "muted": false
}
```

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `VolumeState` |
| `400` | Bad Request | `ProblemDetails` |

### `PUT /api/v1/mixer/snapcast/clients/volume`

**Request body**

Required: **yes**. Content type: `application/json`.

Schema: [`SetVolumeRequest`](#setvolumerequest)

| Field | Type | Constraint |
|---|---|---|
| `volumePercent` | `integer | string (int32)` | **required** (min 0, max 100) |
| `muted` | `boolean` | optional |

Example:

```json
{
  "volumePercent": 50,
  "muted": false
}
```

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | map of `VolumeState` |
| `400` | Bad Request | `ProblemDetails` |

### `PUT /api/v1/mixer/snapcast/clients/{clientId}/name`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `clientId` | path | `string` | yes | — | — |

**Request body**

Required: **yes**. Content type: `application/json`.

Schema: [`SetClientNameRequest`](#setclientnamerequest)

| Field | Type | Constraint |
|---|---|---|
| `name` | `string` | **required** |

Example:

```json
{
  "name": "Kitchen"
}
```

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `string` |
| `400` | Bad Request | `ProblemDetails` |

### `PUT /api/v1/mixer/snapcast/clients/{clientId}/latency`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `clientId` | path | `string` | yes | — | — |

**Request body**

Required: **yes**. Content type: `application/json`.

Schema: [`SetClientLatencyRequest`](#setclientlatencyrequest)

| Field | Type | Constraint |
|---|---|---|
| `latency` | `integer | string (int32)` | **required** |

Example:

```json
{
  "latency": 0
}
```

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `integer | string (int32)` |
| `400` | Bad Request | `ProblemDetails` |

### `DELETE /api/v1/mixer/snapcast/clients/{clientId}`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `clientId` | path | `string` | yes | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `204` | No Content | `object` (unspecified) |

### `PUT /api/v1/mixer/snapcast/groups/{groupId}/mute`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `groupId` | path | `string` | yes | — | — |

**Request body**

Required: **yes**. Content type: `application/json`.

Schema: [`SetGroupMuteRequest`](#setgroupmuterequest)

| Field | Type | Constraint |
|---|---|---|
| `mute` | `boolean` | **required** |

Example:

```json
{
  "mute": false
}
```

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `boolean` |
| `400` | Bad Request | `ProblemDetails` |

### `PUT /api/v1/mixer/snapcast/groups/{groupId}/stream`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `groupId` | path | `string` | yes | — | — |

**Request body**

Required: **yes**. Content type: `application/json`.

Schema: [`SetGroupStreamRequest`](#setgroupstreamrequest)

| Field | Type | Constraint |
|---|---|---|
| `streamId` | `string` | **required** |

Example:

```json
{
  "streamId": "default"
}
```

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `string` |
| `400` | Bad Request | `ProblemDetails` |

### `PUT /api/v1/mixer/snapcast/groups/{groupId}/clients`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `groupId` | path | `string` | yes | — | — |

**Request body**

Required: **yes**. Content type: `application/json`.

Schema: [`SetGroupClientsRequest`](#setgroupclientsrequest)

| Field | Type | Constraint |
|---|---|---|
| `clientIds` | `array<string>` | **required** |

Example:

```json
{
  "clientIds": [
    "string"
  ]
}
```

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `array<string>` |
| `400` | Bad Request | `ProblemDetails` |

---

## Spotify Auth

OAuth 2 authorization-code flow used by the Spotify catalog and library endpoints.

### `GET /api/v1/auth/login`

**Responses**

| Status | Description | Body |
|---|---|---|
| `302` | Found | `object` (unspecified) |

### `GET /api/v1/auth/callback`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `code` | query | `string` | no | — | — |
| `state` | query | `string` | no | — | — |
| `error` | query | `string` | no | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |
| `400` | Bad Request | `ProblemDetails` |
| `401` | Unauthorized | `ProblemDetails` |

### `POST /api/v1/auth/refresh`

**Request body**

Required: **yes**. Content type: `application/json`.

Schema: `string`

The body is a raw JSON string (for example a refresh token).

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `object` (unspecified) |
| `400` | Bad Request | `ProblemDetails` |

---

## Spotify

Spotify catalog objects (albums, tracks, shows, episodes, playlists) and the current user’s saved library. Requires a completed auth flow.

### `GET /api/v1/media/spotify/albums/{id}`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `id` | path | `string` | yes | — | — |
| `market` | query | `string` | no | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `MediaAlbum` |
| `404` | Not Found | `ProblemDetails` |

### `GET /api/v1/media/spotify/albums/{id}/tracks`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `id` | path | `string` | yes | — | — |
| `market` | query | `string` | no | — | — |
| `limit` | query | `integer | string (int32)` | no | — | — |
| `offset` | query | `integer | string (int32)` | no | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | array of `MediaMetaData` |
| `404` | Not Found | `ProblemDetails` |

### `GET /api/v1/media/spotify/shows/{id}`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `id` | path | `string` | yes | — | — |
| `market` | query | `string` | no | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `MediaMetaData` |
| `404` | Not Found | `ProblemDetails` |

### `GET /api/v1/media/spotify/shows/{id}/episodes`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `id` | path | `string` | yes | — | — |
| `market` | query | `string` | no | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | array of `MediaMetaData` |
| `404` | Not Found | `ProblemDetails` |

### `GET /api/v1/media/spotify/episodes/{id}`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `id` | path | `string` | yes | — | — |
| `market` | query | `string` | no | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `MediaMetaData` |
| `404` | Not Found | `ProblemDetails` |

### `GET /api/v1/media/spotify/tracks/{id}`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `id` | path | `string` | yes | — | — |
| `market` | query | `string` | no | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `MediaMetaData` |
| `404` | Not Found | `ProblemDetails` |

### `GET /api/v1/media/spotify/me`

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `SpotifyResponseOfSpotifyUserProfile` |

### `GET /api/v1/media/spotify/playlists/{playlistId}`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `playlistId` | path | `string` | yes | — | — |
| `market` | query | `string` | no | — | — |
| `fields` | query | `string` | no | — | — |
| `additionalTypes` | query | `string` | no | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `MediaMetaData` |
| `404` | Not Found | `ProblemDetails` |

### `GET /api/v1/media/spotify/playlists/{playlistId}/items`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `playlistId` | path | `string` | yes | — | — |
| `market` | query | `string` | no | — | — |
| `fields` | query | `string` | no | — | — |
| `additionalTypes` | query | `string` | no | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | array of `MediaMetaData` |

### `GET /api/v1/media/spotify/me/playlists`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `limit` | query | `integer | string (int32)` | no | — | — |
| `offset` | query | `integer | string (int32)` | no | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `SpotifyResponseOfSpotifyPagedResultOfSpotifyPlaylist` |

### `GET /api/v1/media/spotify/me/albums`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `limit` | query | `integer | string (int32)` | no | — | — |
| `offset` | query | `integer | string (int32)` | no | — | — |
| `market` | query | `string` | no | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `SpotifyResponseOfSpotifyPagedResultOfSpotifySavedAlbum` |

### `GET /api/v1/media/spotify/me/tracks`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `market` | query | `string` | no | — | — |
| `limit` | query | `integer | string (int32)` | no | — | — |
| `offset` | query | `integer | string (int32)` | no | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `SpotifyResponseOfSpotifyPagedResultOfSpotifySavedTrack` |

### `GET /api/v1/media/spotify/me/episodes`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `market` | query | `string` | no | — | — |
| `limit` | query | `integer | string (int32)` | no | — | — |
| `offset` | query | `integer | string (int32)` | no | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `SpotifyResponseOfSpotifyPagedResultOfSpotifySavedEpisode` |

### `GET /api/v1/media/spotify/me/shows`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `limit` | query | `integer | string (int32)` | no | — | — |
| `offset` | query | `integer | string (int32)` | no | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `SpotifyResponseOfSpotifyPagedResultOfSpotifySavedShow` |

---

## TuneIn

TuneIn radio directory: browse outlines, search, station describe, and tune (resolve stream URLs).

### `GET /api/v1/media/tunein/browse`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `category` | query | `string` | no | — | — |
| `id` | query | `string` | no | — | — |
| `filter` | query | `string` | no | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `TuneInResponseOfTuneInOutline` |
| `404` | Not Found | `ProblemDetails` |

### `GET /api/v1/media/tunein/describe/{id}`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `id` | path | `string` | yes | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `TuneInResponseOfStationElement` |
| `404` | Not Found | `ProblemDetails` |

### `GET /api/v1/media/tunein/search`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `query` | query | `string` | yes | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `TuneInOutline` |
| `404` | Not Found | `ProblemDetails` |

### `GET /api/v1/media/tunein/tune/{id}`

**Parameters**

| Name | In | Type | Required | Default | Description |
|---|---|---|---|---|---|
| `id` | path | `string` | yes | — | — |
| `filter` | query | `string` | no | — | — |

**Responses**

| Status | Description | Body |
|---|---|---|
| `200` | OK | `TuneInResponseOfAudioElement` |
| `404` | Not Found | `ProblemDetails` |

---

## Common schemas

Only fields declared on each schema are listed. Nested Spotify and TuneIn vendor objects follow the upstream APIs and are included for completeness at the end of this section.

### `MediaMetaData`

Required: `album`, `title`, `artist`, `url`, `imageUrl`

| Field | Type | Constraint |
|---|---|---|
| `uri` | `null | MediaUri` | optional |
| `rawUri` | `string | null` | optional |
| `album` | `string` | **required** |
| `title` | `string` | **required** |
| `artist` | `string` | **required** |
| `url` | `string` | **required** |
| `imageUrl` | `string` | **required** |
| `duration` | `number | string | null (double)` | optional |

### `MediaUri`

Required: `source`, `type`, `id`

| Field | Type | Constraint |
|---|---|---|
| `source` | `string` | **required** |
| `type` | `string` | **required** |
| `id` | `string` | **required** |
| `secondaryId` | `string | null` | optional |

### `MediaAlbum`

Required: `album`, `title`, `artist`, `url`, `imageUrl`

| Field | Type | Constraint |
|---|---|---|
| `tracks` | `array<MediaMetaData>` | optional |
| `uri` | `null | MediaUri` | optional |
| `rawUri` | `string | null` | optional |
| `album` | `string` | **required** |
| `title` | `string` | **required** |
| `artist` | `string` | **required** |
| `url` | `string` | **required** |
| `imageUrl` | `string` | **required** |
| `duration` | `number | string | null (double)` | optional |

### `PlaybackState`

| Field | Type | Constraint |
|---|---|---|
| `activeBackend` | `string` | optional |
| `track` | `null | MediaMetaData` | optional |
| `position` | `number | string (double)` | optional |
| `playing` | `boolean` | optional |
| `isLive` | `boolean` | optional |

### `PlayRequest`

Required: `uri`

| Field | Type | Constraint |
|---|---|---|
| `uri` | `string` | **required** |

### `SeekRequest`

Required: `position`

| Field | Type | Constraint |
|---|---|---|
| `position` | `string` | **required** |

### `SetVolumeRequest`

Required: `volumePercent`

| Field | Type | Constraint |
|---|---|---|
| `volumePercent` | `integer | string (int32)` | **required** (min 0, max 100) |
| `muted` | `boolean` | optional |

### `VolumeState`

Required: `percent`, `muted`

| Field | Type | Constraint |
|---|---|---|
| `percent` | `integer | string (int32)` | **required** |
| `muted` | `boolean` | **required** |

### `PagedListOfMediaMetaData`

| Field | Type | Constraint |
|---|---|---|
| `offset` | `integer | string (int32)` | optional |
| `pageNumber` | `integer | string (int32)` | optional |
| `totalPages` | `integer | string (int32)` | optional |
| `items` | `array<MediaMetaData> | null` | optional |
| `limit` | `integer | string (int32)` | optional |
| `totalCount` | `integer | string (int32)` | optional |
| `hasPreviousPage` | `boolean` | optional |
| `hasNextPage` | `boolean` | optional |

### `ProblemDetails`

| Field | Type | Constraint |
|---|---|---|
| `type` | `string | null` | optional |
| `title` | `string | null` | optional |
| `status` | `integer | string | null (int32)` | optional |
| `detail` | `string | null` | optional |
| `instance` | `string | null` | optional |

### `ValidationProblemDetails`

| Field | Type | Constraint |
|---|---|---|
| `type` | `string | null` | optional |
| `title` | `string | null` | optional |
| `status` | `integer | string | null (int32)` | optional |
| `detail` | `string | null` | optional |
| `instance` | `string | null` | optional |
| `errors` | `map<string, array<string>>` | optional |

### `ApiPlayRequest`

Required: `uri`

| Field | Type | Constraint |
|---|---|---|
| `uri` | `string` | **required** |
| `skip_to_uri` | `string | null` | optional |
| `paused` | `boolean | null` | optional |
| `position` | `integer | string | null (int64)` | optional |

### `ApiSeekRequest`

Required: `position`

| Field | Type | Constraint |
|---|---|---|
| `position` | `integer | string (int64)` | **required** |
| `relative` | `boolean` | optional |

### `ApiShuffleContextRequest`

Required: `shuffle_context`

| Field | Type | Constraint |
|---|---|---|
| `shuffle_context` | `boolean` | **required** |

### `ApiRepeatContextRequest`

Required: `repeat_context`

| Field | Type | Constraint |
|---|---|---|
| `repeat_context` | `boolean` | **required** |

### `ApiRepeatTrackRequest`

Required: `repeat_track`

| Field | Type | Constraint |
|---|---|---|
| `repeat_track` | `boolean` | **required** |

### `MpdPlayRequest`

Required: `fileOrUrl`

| Field | Type | Constraint |
|---|---|---|
| `fileOrUrl` | `string` | **required** |

### `MpdCommandRequest`

Required: `command`

| Field | Type | Constraint |
|---|---|---|
| `command` | `string` | **required** |

### `SnapServer`

Required: `host`, `groups`, `streams`

| Field | Type | Constraint |
|---|---|---|
| `host` | `ClientHost` | **required** |
| `groups` | `array<SnapGroup>` | **required** |
| `streams` | `array<SnapStream>` | **required** |

### `SnapGroup`

Required: `id`, `name`, `muted`, `stream_id`, `clients`

| Field | Type | Constraint |
|---|---|---|
| `id` | `string` | **required** |
| `name` | `string` | **required** |
| `muted` | `boolean` | **required** |
| `stream_id` | `string` | **required** |
| `clients` | `array<SnapClient>` | **required** |

### `SnapClient`

Required: `id`, `connected`, `config`, `host`, `lastSeen`

| Field | Type | Constraint |
|---|---|---|
| `id` | `string` | **required** |
| `connected` | `boolean` | **required** |
| `config` | `ClientConfig` | **required** |
| `host` | `ClientHost` | **required** |
| `lastSeen` | `SnapTime` | **required** |

### `SnapStream`

Required: `id`, `status`, `uri`

| Field | Type | Constraint |
|---|---|---|
| `id` | `string` | **required** |
| `status` | `string` | **required** |
| `uri` | `JsonElement` | **required** |

### `ClientConfig`

Required: `instance`, `latency`, `name`, `volume`

| Field | Type | Constraint |
|---|---|---|
| `instance` | `integer | string (int32)` | **required** |
| `latency` | `integer | string (int32)` | **required** |
| `name` | `string` | **required** |
| `volume` | `VolumeState` | **required** |

### `ClientHost`

Required: `arch`, `ip`, `mac`, `name`, `os`

| Field | Type | Constraint |
|---|---|---|
| `arch` | `string` | **required** |
| `ip` | `string` | **required** |
| `mac` | `string` | **required** |
| `name` | `string` | **required** |
| `os` | `string` | **required** |

### `SnapTime`

Required: `sec`, `usec`

| Field | Type | Constraint |
|---|---|---|
| `sec` | `integer | string (int64)` | **required** |
| `usec` | `integer | string (int64)` | **required** |

### `RpcVersion`

Required: `major`, `minor`, `patch`

| Field | Type | Constraint |
|---|---|---|
| `major` | `integer | string (int32)` | **required** |
| `minor` | `integer | string (int32)` | **required** |
| `patch` | `integer | string (int32)` | **required** |

### `SetClientNameRequest`

Required: `name`

| Field | Type | Constraint |
|---|---|---|
| `name` | `string` | **required** |

### `SetClientLatencyRequest`

Required: `latency`

| Field | Type | Constraint |
|---|---|---|
| `latency` | `integer | string (int32)` | **required** |

### `SetGroupMuteRequest`

Required: `mute`

| Field | Type | Constraint |
|---|---|---|
| `mute` | `boolean` | **required** |

### `SetGroupStreamRequest`

Required: `streamId`

| Field | Type | Constraint |
|---|---|---|
| `streamId` | `string` | **required** |

### `SetGroupClientsRequest`

Required: `clientIds`

| Field | Type | Constraint |
|---|---|---|
| `clientIds` | `array<string>` | **required** |

### `ServerStats`

| Field | Type | Constraint |
|---|---|---|
| `supported_version` | `int32` | optional |
| `software_version` | `string | null` | optional |
| `status` | `string | null` | optional |
| `stations` | `int32` | optional |
| `stations_broken` | `int32` | optional |
| `tags` | `int32` | optional |
| `clicks_last_hour` | `int32` | optional |
| `clicks_last_day` | `int32` | optional |
| `languages` | `int32` | optional |
| `countries` | `int32` | optional |

### `ApiServer`

| Field | Type | Constraint |
|---|---|---|
| `ip` | `string | null` | optional |
| `name` | `string | null` | optional |

### `ClickResult`

| Field | Type | Constraint |
|---|---|---|
| `ok` | `object` | optional |
| `message` | `string | null` | optional |
| `stationuuid` | `string | null` | optional |
| `name` | `string | null` | optional |
| `url` | `string | null` | optional |

### Spotify wrapper and catalog types

Spotify endpoints that hit `/me/*` wrap upstream payloads in `SpotifyResponseOf…` objects with `isSuccess`, `statusCode`, `data`, and `error` (`SpotifyErrorObject`). Catalog GET endpoints for albums, tracks, shows, episodes, and playlists generally return Viox `MediaMetaData` / `MediaAlbum` rather than raw Spotify objects, except `/me` which returns `SpotifyResponseOfSpotifyUserProfile`.

Declared Spotify schemas: `SpotifyAlbum`, `SpotifyArtist`, `SpotifyTrack`, `SpotifyEpisode`, `SpotifyShow`, `SpotifyPlaylist`, `SpotifyPlaylistItem`, `SpotifyUserProfile`, `SpotifyImage`, `SpotifyExternalUrls`, `SpotifyFollowers`, `SpotifyErrorObject`, saved-item wrappers (`SpotifySavedAlbum`, `SpotifySavedTrack`, `SpotifySavedEpisode`, `SpotifySavedShow`), paged results (`SpotifyPagedResultOf…`), and response envelopes (`SpotifyResponseOf…`).

### TuneIn types

TuneIn responses wrap a `head` (`TuneInHeader`: `title`, `status`) and a `body` array.

- `TuneInOutline` — recursive browse/search node (`type`, `key`, `text`, `URL`, `guide_id`, `image`, `children`, stream hints).
- `StationElement` — rich station record (call sign, slogan, geo, now-playing, ads, favorites, …).
- `AudioElement` — resolved stream (`url`, `bitrate`, `media_type`, HLS / live-seek flags).
- Envelopes: `TuneInResponseOfTuneInOutline`, `TuneInResponseOfStationElement`, `TuneInResponseOfAudioElement`.

---

## Error model

### `ProblemDetails`

| Field | Type |
|---|---|
| `type` | `string \| null` |
| `title` | `string \| null` |
| `status` | `int32 \| null` |
| `detail` | `string \| null` |
| `instance` | `string \| null` |

### `ValidationProblemDetails`

Same fields as `ProblemDetails`, plus:

| Field | Type |
|---|---|
| `errors` | `map<string, string[]>` |

Used by media search when `query` or paging values fail validation.

Common status codes in this spec:

| Code | Meaning |
|---|---|
| `200` | Success. |
| `204` | Success, no body (Snapcast client delete). |
| `302` | Redirect (Spotify login). |
| `400` | Bad request / validation. |
| `401` | Unauthorized (Spotify callback). |
| `404` | Unknown source, station, or catalog id. |
| `500` | Upstream or backend failure (MPD, Snapcast connect, search). |

---

## Endpoint index

| Method | Path | Tag |
|---|---|---|
| `GET` | `/api/v1/library/{source}` | Library |
| `GET` | `/api/v1/players/librespot/state` | Librespot |
| `GET` | `/api/v1/players/librespot/events` | Librespot |
| `POST` | `/api/v1/players/librespot/play` | Librespot |
| `POST` | `/api/v1/players/librespot/pause` | Librespot |
| `POST` | `/api/v1/players/librespot/resume` | Librespot |
| `POST` | `/api/v1/players/librespot/seek` | Librespot |
| `POST` | `/api/v1/players/librespot/shuffle` | Librespot |
| `POST` | `/api/v1/players/librespot/repeat-context` | Librespot |
| `POST` | `/api/v1/players/librespot/repeat-track` | Librespot |
| `POST` | `/api/v1/media-player/play` | Media Player |
| `POST` | `/api/v1/media-player/pause` | Media Player |
| `POST` | `/api/v1/media-player/resume` | Media Player |
| `POST` | `/api/v1/media-player/stop` | Media Player |
| `POST` | `/api/v1/media-player/seek` | Media Player |
| `POST` | `/api/v1/media-player/next` | Media Player |
| `POST` | `/api/v1/media-player/previous` | Media Player |
| `PUT` | `/api/v1/media-player/volume` | Media Player |
| `GET` | `/api/v1/media-player/volume` | Media Player |
| `GET` | `/api/v1/media-player/active` | Media Player |
| `GET` | `/api/v1/media-player/current` | Media Player |
| `GET` | `/api/v1/media/search` | Media Search |
| `GET` | `/api/v1/media/search/{sourceKey}` | Media Search |
| `POST` | `/api/v1/mpd/connect` | MPD |
| `POST` | `/api/v1/mpd/disconnect` | MPD |
| `POST` | `/api/v1/mpd/play` | MPD |
| `POST` | `/api/v1/mpd/play-file` | MPD |
| `POST` | `/api/v1/mpd/pause` | MPD |
| `POST` | `/api/v1/mpd/stop` | MPD |
| `POST` | `/api/v1/mpd/next` | MPD |
| `POST` | `/api/v1/mpd/previous` | MPD |
| `DELETE` | `/api/v1/mpd/playlist` | MPD |
| `POST` | `/api/v1/mpd/command` | MPD |
| `GET` | `/api/v1/media/podverse/podcast/{podcastId}/episodes` | Podverse |
| `GET` | `/api/v1/media/podverse/podcasts/{podcastId}` | Podverse |
| `GET` | `/api/v1/media/podverse/episodes/{episodeId}` | Podverse |
| `GET` | `/api/v1/media/radiobrowser/stats` | Radio Browser |
| `GET` | `/api/v1/media/radiobrowser/servers` | Radio Browser |
| `GET` | `/api/v1/media/radiobrowser/stations/{stationUuid}` | Radio Browser |
| `GET` | `/api/v1/media/radiobrowser/stations/top-clicked` | Radio Browser |
| `GET` | `/api/v1/media/radiobrowser/stations/top-voted` | Radio Browser |
| `GET` | `/api/v1/media/radiobrowser/stations/recently-clicked` | Radio Browser |
| `POST` | `/api/v1/media/radiobrowser/stations/{stationUuid}/click` | Radio Browser |
| `POST` | `/api/v1/mixer/snapcast/connect` | Snapcast |
| `POST` | `/api/v1/mixer/snapcast/disconnect` | Snapcast |
| `GET` | `/api/v1/mixer/snapcast/rpc-version` | Snapcast |
| `GET` | `/api/v1/mixer/snapcast/status` | Snapcast |
| `GET` | `/api/v1/mixer/snapcast/clients` | Snapcast |
| `PUT` | `/api/v1/mixer/snapcast/clients/{clientId}/volume` | Snapcast |
| `PUT` | `/api/v1/mixer/snapcast/clients/volume` | Snapcast |
| `PUT` | `/api/v1/mixer/snapcast/clients/{clientId}/name` | Snapcast |
| `PUT` | `/api/v1/mixer/snapcast/clients/{clientId}/latency` | Snapcast |
| `DELETE` | `/api/v1/mixer/snapcast/clients/{clientId}` | Snapcast |
| `PUT` | `/api/v1/mixer/snapcast/groups/{groupId}/mute` | Snapcast |
| `PUT` | `/api/v1/mixer/snapcast/groups/{groupId}/stream` | Snapcast |
| `PUT` | `/api/v1/mixer/snapcast/groups/{groupId}/clients` | Snapcast |
| `GET` | `/api/v1/auth/login` | Spotify Auth |
| `GET` | `/api/v1/auth/callback` | Spotify Auth |
| `POST` | `/api/v1/auth/refresh` | Spotify Auth |
| `GET` | `/api/v1/media/spotify/albums/{id}` | Spotify |
| `GET` | `/api/v1/media/spotify/albums/{id}/tracks` | Spotify |
| `GET` | `/api/v1/media/spotify/shows/{id}` | Spotify |
| `GET` | `/api/v1/media/spotify/shows/{id}/episodes` | Spotify |
| `GET` | `/api/v1/media/spotify/episodes/{id}` | Spotify |
| `GET` | `/api/v1/media/spotify/tracks/{id}` | Spotify |
| `GET` | `/api/v1/media/spotify/me` | Spotify |
| `GET` | `/api/v1/media/spotify/playlists/{playlistId}` | Spotify |
| `GET` | `/api/v1/media/spotify/playlists/{playlistId}/items` | Spotify |
| `GET` | `/api/v1/media/spotify/me/playlists` | Spotify |
| `GET` | `/api/v1/media/spotify/me/albums` | Spotify |
| `GET` | `/api/v1/media/spotify/me/tracks` | Spotify |
| `GET` | `/api/v1/media/spotify/me/episodes` | Spotify |
| `GET` | `/api/v1/media/spotify/me/shows` | Spotify |
| `GET` | `/api/v1/media/tunein/browse` | TuneIn |
| `GET` | `/api/v1/media/tunein/describe/{id}` | TuneIn |
| `GET` | `/api/v1/media/tunein/search` | TuneIn |
| `GET` | `/api/v1/media/tunein/tune/{id}` | TuneIn |

---

*Generated from the Viox.Net OpenAPI 3.1.1 description (`v1.0`).*
