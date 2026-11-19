# AccountWeb

Small web front end for the AccountServer. Sign in with an admin DarkFactor account,
then create new users (username, nickname, password and optional email).

Users are created through the AccountServer's `CreateAccount` API with the AccountClient, so
they get the same validation, salting and hashing as accounts created anywhere else. AccountWeb
has no database connection of its own.

Layout and stack follow the DarkAIFactory dashboard: ASP.NET Core minimal API + React,
Tailwind and Vite, cookie session, light/dark theme.

## Configuration

Copy `Config/appsettings.Template.json` to `Config/appsettings.<Environment>.json` and fill in the
AccountServer endpoint and the OAuth2 client credentials AccountWeb uses to call it.

### Admin users

Only accounts listed in `AdminUsers` can sign in and create users:

```json
"AdminUsers": [ "alice", "bob" ]
```

- Names are login usernames (not nicknames), compared case-insensitively.
- The list is checked at sign-in and on every request, so removing a name locks that user
  out on their next request. Config changes need a restart.
- An empty list means nobody can sign in (login answers 503 "No admin users are configured").
- A valid account that is not on the list gets 403 "This account is not allowed to use AccountWeb".

### First-run setup (creating the first admin)

While `AdminUsers` is empty, AccountWeb shows a **Create the first account** page instead of the
login page:

1. Open AccountWeb **from the machine it runs on** (`http://localhost:...`) and create the account.
2. Add the username to `AdminUsers` in `Config/appsettings.<Environment>.json` (the page shows the exact file).
3. Restart AccountWeb and sign in.

Safeguards: setup is only offered to loopback requests, only while `AdminUsers` is empty, and
only for **one** account per process start (later requests get 403 until restart).

In Docker, browser requests arrive from the Docker gateway, not loopback. Set
`"AllowRemoteSetup": true` for the first run, then set it back to `false` together with adding
the admin name. Never leave it on for a server reachable from outside.

| Method | Path                       | Auth | Purpose |
|--------|----------------------------|------|---------|
| GET    | `/api/setup/status`        | none | `{ setupMode, configFile? }` |
| POST   | `/api/setup/first-account` | none (setup only) | Create the first account |

## Run locally

```sh
# Backend (http://localhost:5000)
dotnet run --project AccountWeb/AccountWeb.csproj --environment Development --urls http://localhost:5000

# Frontend dev server with API proxy (http://localhost:5174)
cd AccountWeb
npm install
npm run dev
```

`npm run build` writes the frontend to `dist/`; the Docker image serves it from `wwwroot/`.

## API

| Method | Path               | Auth   | Purpose                        |
|--------|--------------------|--------|--------------------------------|
| GET    | `/api/health`      | none   | Health check                   |
| POST   | `/api/auth/login`  | none   | Sign in `{username, password}` |
| POST   | `/api/auth/logout` | none   | Sign out                       |
| GET    | `/api/auth/me`     | cookie | Current user                   |
| POST   | `/api/accounts`    | cookie | Create user `{username, password, nickname, email?}` |

## Docker

```sh
docker build -f AccountWeb/Dockerfile --secret id=nuget_username,env=NUGET_USERNAME --secret id=nuget_token,env=NUGET_TOKEN .
```

or `docker compose up accountweb` from the repository root (port 5101).
