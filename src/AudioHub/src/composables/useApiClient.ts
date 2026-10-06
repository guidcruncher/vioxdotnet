import { useApiTransport } from './useApiTransport'

import { createQueueModule } from './modules/queue'
import { createCoreModule } from './modules/core'
import { createConfigModule } from './modules/config'
import { createMediaModule } from './modules/media'
import { createPlaylistsModule } from './modules/playlists'
import { createFilePlaylistsModule } from './modules/filePlaylists'
import { createRadioBrowserModule } from './modules/radioBrowser'
import { createFavouritesModule } from './modules/favourites'
import { createLibraryModule } from './modules/library'
import { createEqualizerModule } from './modules/equalizer'
import { createSnapcastModule } from './modules/snapcast'
import { createAuthModule } from './modules/auth'
import { createEventsModule } from './modules/events'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? ''

export function useApiClient(baseUrl: string = apiBaseUrl) {
  const transport = useApiTransport(baseUrl)

  return {
    createApiState: transport.createApiState,

    core: createCoreModule(transport),
    queue: createQueueModule(transport),
    config: createConfigModule(transport),
    media: createMediaModule(transport),
    playlists: createPlaylistsModule(transport),
    filePlaylists: createFilePlaylistsModule(transport),
    radioBrowser: createRadioBrowserModule(transport),
    favourites: createFavouritesModule(transport),
    library: createLibraryModule(transport),
    equalizer: createEqualizerModule(transport),
    snapcast: createSnapcastModule(transport),
    auth: createAuthModule(transport, baseUrl),
    events: createEventsModule(transport),
  }
}
