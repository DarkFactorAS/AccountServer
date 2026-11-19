# AccountWeb

Small web front end for the AccountServer. Sign in with an existing DarkFactor account,
then create new users (username, nickname, password and optional email).

Users are created through the AccountServer's `CreateAccount` API with the AccountClient, so
they get the same validation, salting and hashing as accounts created anywhere else. AccountWeb
has no database connection of its own.

Layout and stack follow the DarkAIFactory dashboard: ASP.NET Core minimal API + React,
Tailwind and Vite, cookie session, light/dark theme.

## Configuration

Copy `Config/appsettings.Template.json` to `Config/appsettings.<Environment>.json` and fill in the
AccountServer endpoint and the OAuth2 client credentials AccountWeb uses to call it.

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
