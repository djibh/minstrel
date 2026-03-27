# Minstrel Backend

API backend **.NET** de Minstrel.

Le backend joue le rôle de **façade multi-source** :

- il dialogue avec les services externes
- il normalise les données
- il expose une API HTTP/JSON stable au mobile
- il prépare les futures extensions à d’autres providers

Le frontend mobile **ne dialogue jamais directement avec les services externes**.

## Objectif

Le backend Minstrel doit :

- agréger les sources média
- normaliser albums / artistes / morceaux / playlists
- exposer les endpoints de bibliothèque et de recherche
- fournir les informations de lecture
- préparer la gestion offline
- rester extensible pour d’autres providers que pCloud

## Stack technique

- **.NET / ASP.NET Core Web API**
- **Controllers**
- **Dependency Injection**
- **Swagger / OpenAPI**
- provider mock/in-memory pour le démarrage
- future implémentation réelle `pCloud`

## Architecture

Le backend suit une séparation en couches :

- **Api** : contrôleurs HTTP, contrats, bootstrap
- **Application** : orchestration, agrégation, cas d’usage
- **Domain** : modèles métier normalisés et interfaces
- **Infrastructure** : providers externes, config, persistance

## Structure de la solution

```text
backend/
  Minstrel.sln

  src/
    Minstrel.Api/
    Minstrel.Application/
    Minstrel.Domain/
    Minstrel.Infrastructure/

  tests/
    Minstrel.UnitTests/
    Minstrel.IntegrationTests/
```

## Structure détaillée

```text
src/
  Minstrel.Api/
    Controllers/
      LibraryController.cs
      SearchController.cs
      PlaybackController.cs
      SourcesController.cs
    Contracts/
      Albums/
      Artists/
      Tracks/
      Playlists/
      Search/
      Sources/
    Mapping/
      ApiContractMapper.cs
    Extensions/
      ServiceCollectionExtensions.cs
    Program.cs
    appsettings.json

  Minstrel.Application/
    Abstractions/
      Providers/
        ISourceRegistry.cs
    Library/
      Services/
        LibraryAggregationService.cs
    Search/
      Services/
        SearchAggregationService.cs
    Playback/
      Services/
        PlaybackService.cs

  Minstrel.Domain/
    Entities/
      Album.cs
      Artist.cs
      Track.cs
      Playlist.cs
      MediaSource.cs
      SearchResults.cs
    Enums/
      SourceKind.cs
      SourceSyncStatus.cs
    Interfaces/
      IMediaSourceProvider.cs
    ValueObjects/
      StreamDescriptor.cs

  Minstrel.Infrastructure/
    DependencyInjection/
      InfrastructureServiceCollectionExtensions.cs
    Providers/
      Mock/
        MockMediaSourceProvider.cs
      PCloud/
        PCloudClient.cs
        PCloudProvider.cs
        PCloudMapper.cs
    Sources/
      SourceRegistry.cs
```

## Contrat fonctionnel

Le backend expose des données normalisées, indépendantes des providers.

### Endpoints MVP

- `GET /sources`
- `GET /library/albums`
- `GET /library/artists`
- `GET /library/tracks`
- `GET /library/playlists`
- `GET /search?q=...`
- `GET /playback/tracks/{id}/stream`

### Endpoints à venir

- `GET /albums/{id}`
- `GET /albums/{id}/tracks`
- `GET /artists/{id}`
- `GET /artists/{id}/tracks`
- `GET /playlists/{id}`
- `GET /playlists/{id}/tracks`
- `GET /offline/storage`
- `GET /offline/items`
- `POST /offline/tracks/{id}`
- `POST /offline/albums/{id}`

## Provider model

Le backend repose sur une interface de provider du type :

- `GetSourceAsync`
- `GetAlbumsAsync`
- `GetArtistsAsync`
- `GetTracksAsync`
- `GetPlaylistsAsync`
- `SearchAsync`
- `GetTrackStreamAsync`

Chaque provider externe est encapsulé dans `Infrastructure/Providers/...`.

## Source externe planifiée

Pour le MVP, seule **pCloud** est planifiée comme source externe réelle.

Mais l’architecture est volontairement pensée pour permettre l’ajout futur de :

- autres clouds
- serveurs personnels
- autres providers compatibles

## État actuel

À ce stade, le backend permet déjà :

- de démarrer correctement
- d’exposer Swagger
- de servir des données mock via un provider in-memory
- de répondre aux endpoints de bibliothèque, recherche, playback et sources

## Lancement

Depuis le dossier backend :

```bash
dotnet run --project src/Minstrel.Api
```

Ou avec un profil précis :

```bash
dotnet run --project src/Minstrel.Api --launch-profile http-local-network
```

## Swagger

En développement, Swagger est accessible à une URL du type :

```text
http://localhost:5063/swagger
```

ou via l’IP locale si l’API écoute sur le réseau local :

```text
http://192.168.1.80:5063/swagger
```

## Configuration locale

Le backend peut être configuré pour écouter sur le réseau local afin d’être joignable depuis l’app Expo.

Exemple de profil `launchSettings.json` :

```json
{
  "profiles": {
    "http-local-network": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://0.0.0.0:5063",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

## CORS

Pour les tests en **Expo Web**, le backend doit autoriser l’origine du frontend.

En développement, une politique permissive peut être utilisée temporairement.

## Conventions

- `Dto` ou `Response` pour les contrats API
- entités domaine sans suffixe DTO
- contrôleurs minces
- logique d’orchestration dans `Application`
- logique provider dans `Infrastructure`
- aucune logique provider dans le frontend

## Roadmap technique

### En cours

- provider mock
- endpoints MVP
- intégration avec le front Expo

### À venir

- implémentation réelle pCloud
- détails album / artiste / playlist
- offline
- cache
- tests d’intégration enrichis
- meilleure gestion du stream playback

## Notes

- Le backend est la source de vérité normalisée pour le mobile.
- Le mobile doit rester agnostique vis-à-vis des providers.
- Toute future source externe doit idéalement être intégrée côté backend, pas côté app.
