# YouTube Fast — Jellyfin plugin

Same idea as YouTubeSync (`.strm` + `.nfo`, on-demand playback via yt-dlp),
but indexing is done through the **YouTube Data API v3** instead of scraping.
That makes channel/playlist analysis dramatically faster: ~50 videos per API
call, no throttling.

- **Indexing** → YouTube Data API v3 (fast, paginated JSON)
- **Playback** → yt-dlp, one video at a time, only when you press play

## Legal

This is an independent, community project. It is **not affiliated with,
endorsed by, or sponsored by** YouTube, Google LLC, or Jellyfin.

The plugin does not host, mirror, or permanently store any third-party video.
Indexing only writes metadata, thumbnail images, and small `.strm` pointer
files to your own library folder; the actual stream is resolved on demand, at
playback time, directly from YouTube via `yt-dlp`. Nothing is cached or
redistributed by the project itself.

Use it only with content you have the right to access and to watch this way —
your own uploads, public-domain works, Creative Commons–licensed videos, or
any other content whose rights holder and YouTube's Terms of Service permit
this kind of personal, on-demand playback. You are solely responsible for how
you use this software and for complying with YouTube's Terms of Service,
applicable copyright law, and any other regulation in your jurisdiction.

The software is provided "as is", without warranty of any kind — see
[LICENSE](LICENSE).

## Quick install (Jellyfin plugin repository)

1. **Dashboard → Plugins → Repositories → Add Repository**.
2. Repository name: `YouTube Fast`. Repository URL:
   `https://raw.githubusercontent.com/RM4Y/YouTubeFast/main/manifest.json`
3. **Dashboard → Plugins → Catalog**, find **YouTube Fast**, install it, then
   restart Jellyfin.

This pulls the latest release automatically — no manual build/deploy needed.
Skip straight to [step 4 (Configure)](#4-configure) once installed. The steps
below (Build/Deploy) are only needed if you want to build from source instead.

## 1. Get a YouTube Data API key (free)

1. Go to <https://console.cloud.google.com/>.
2. Create a project (or reuse one).
3. **APIs & Services → Library → search "YouTube Data API v3" → Enable**.
4. **APIs & Services → Credentials → Create credentials → API key**.
5. Copy the key. (Optional: restrict it to the YouTube Data API v3.)

Free quota is 10,000 units/day. This plugin avoids the expensive `search`
endpoint, so a typical setup uses only a handful of units per sync.

## 2. Build

Requires the .NET 9 SDK.

```bash
dotnet publish Jellyfin.Plugin.YouTubeFast/Jellyfin.Plugin.YouTubeFast.csproj \
  -c Release --no-self-contained -o publish/
```

The output folder contains `Jellyfin.Plugin.YouTubeFast.dll`.

> If you are **not** on Jellyfin 10.11.x, edit the two `<PackageReference>`
> versions in the `.csproj` AND `targetAbi` in `meta.json` to match your
> server, then rebuild.

## 3. Deploy

```bash
PLUGIN_DIR="/config/plugins/YouTubeFast"   # path inside your Jellyfin container/host
mkdir -p "$PLUGIN_DIR"
cp publish/Jellyfin.Plugin.YouTubeFast.dll "$PLUGIN_DIR/"
cp Jellyfin.Plugin.YouTubeFast/meta.json   "$PLUGIN_DIR/"
```

Restart Jellyfin. The plugin appears under **Dashboard → Plugins**.

Make sure **yt-dlp** is installed and on PATH inside the same
container/host (used only at playback time):

```bash
yt-dlp --version
```

## 4. Configure

**Dashboard → Plugins → YouTube Fast**:

- Paste your **API key**.
- Set the **Library folder** to a path inside a Jellyfin library
  (e.g. `/media/youtube`).
- Set the **Jellyfin address** to the URL your devices use to reach the server.
- Add channels/playlists. For each, pick **Episodes in a series**
  (channel = series, year = season) or **Separate movies**.

Then create/point a Jellyfin library at that folder:
- **Series mode** → library type **Shows**
- **Movies mode** → library type **Movies**

> Keep series-mode sources and movies-mode sources in **separate** library
> folders so the content type matches — mixing them in one library is what
> causes the "everything jumbled" problem.

Run **Dashboard → Scheduled Tasks → Sync YouTube (Fast)** once, then let the
6-hour schedule keep it fresh. Later syncs only process new videos, so they're
fast.

## How playback works

Each `.strm` points to `http://<your-server>/YouTubeFast/Stream/<videoId>`.
When you press play, the plugin's controller runs yt-dlp to resolve a real
stream URL and 302-redirects to it. Resolved URLs are cached briefly.

The resolver endpoint is `[AllowAnonymous]` so the transcoder/clients can fetch
it without an auth token. Anyone who can reach the endpoint can have your server
resolve a YouTube URL — keep it on a trusted network, or add auth and a token to
the `.strm` URLs if you expose Jellyfin publicly.

## Known limits

- No cookie support → age-restricted / members-only videos won't resolve.
- `/c/CustomName` channel URLs aren't directly resolvable by the API; paste the
  `/channel/UC…` or `/@handle` form instead.
- Tested-by-design against Jellyfin 10.11.x; other versions need the version
  bump described in step 2.

## Project layout

```
Plugin.cs                         entry point + config page registration
Configuration/PluginConfiguration.cs   settings model
Configuration/configPage.html     settings UI
YouTube/YouTubeApiClient.cs       Data API v3 client (the fast indexer)
YouTube/Models.cs                 API DTOs + normalised types
ScheduledTasks/YouTubeSyncTask.cs orchestrates the sync (every 6h)
Services/LibraryWriter.cs         writes folders/.strm/.nfo/thumbnails
Services/PlaybackResolver.cs      yt-dlp resolution + URL cache
Api/PlaybackController.cs         /YouTubeFast/Stream/{id} resolver endpoint
```

## Self-service page (users add their own channels)

Users can manage their own YouTube channels from a page the plugin serves:

```
https://<your-server>/YouTubeFast/app
```

They sign in with their Jellyfin account, search a YouTuber (up to 20 results),
add/remove channels, and toggle "exclude Shorts" per channel. Each user's
channels sync into a per-user folder: `<LibraryFolder>/<UserName>`.

### Admin setup for per-user libraries

1. For each user, create a Jellyfin library pointing at
   `<LibraryFolder>/<UserName>` (e.g. `/media/youtube/remy`).
2. In **Dashboard → Users → [user] → Access**, restrict library access so each
   user only sees their own.
3. Share the `/YouTubeFast/app` URL with users (bookmarkable).

Notes:
- This is a separate page, not embedded in the Jellyfin library UI — Jellyfin
  plugins cannot inject custom UI into the library browser.
- Channel search uses the YouTube `search.list` endpoint (100 quota units per
  search); results are cached for 30 minutes to conserve quota.
- Access requires a valid Jellyfin login; for a home/family setup the per-user
  scoping is by the signed-in user.

## License

[MIT](LICENSE) — see the Legal section above for usage responsibilities.
