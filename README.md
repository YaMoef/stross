# Stross

A hobby project that will serve as a service to stream music across multiple services such as:
- Spotify
- Youtube
- Soundcloud
- Tidal

# Getting started
1) Clone the repo
2) Run the docker compose with `docker compose up -d`
3) Go to `http://localhost:8080/scalar
4) Add the 2 providers:
```
curl http://localhost:8080/api/v1/providers \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
  "name": "SoundCloud",
  "url": "http://stross.downloader.souncloud:8080"
}'
curl http://localhost:8080/api/v1/providers \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
  "name": "YouTube",
  "url": "http://stross.downloader.yt:8080"
}'
```
5) Use the displayed ids to download tracks on the `/api/v1/music-tracks/download` endpoint
6) Run the following code in Linqpad or Netpad to add a api key for the user
```
string passwordPlainText = "ThisIsThePassword";

User defaultUser = Users
    .Include(u => u.UserApiKeys)
    .FirstOrDefault(u => u.IsDefaultUser) ?? throw new Exception("Default user not found");

if(defaultUser.UserApiKeys.Any())
    throw new Exception("password already set for user");

defaultUser.UserApiKeys.Add(new UserApiKey()
{
    ApiKey = passwordPlainText,
    CreatedAt = DateTime.UtcNow,
    KeyName = "Default",
    CreatedBy = 0
});

SaveChanges();
```
7) Connect with a subsonic client to the api

# MVP
The first release would be nice if it has the following features:
- User management
- SAML or OAuth support
[x] Create playlists
- Have a Subsonic interface
  - EXTRA: support [OpenSubsonic](https://opensubsonic.netlify.app/)
- Download all the music locally
- Have a clean frontend