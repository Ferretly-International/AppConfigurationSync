# CLAUDE.md — AppConfigurationSync

## Project Overview

AppConfigurationSync is a .NET 8 console tool that compares two Azure App Configuration resources and highlights differences. It surfaces keys missing in the destination (red) and keys present but with differing values (green). It can also create a snapshot of the destination before syncing.

## Solution Structure

```
AppConfigurationSync/
├── AppConfigurationSync.csproj   # .NET 8 console app
├── AppConfigurationSync.sln
├── Program.cs                    # All logic (entry point, comparison, snapshot creation)
└── README.md
```

## Tech Stack

- **.NET 8.0** console app
- **Azure.Data.AppConfiguration** — reads settings and creates snapshots
- **Microsoft.Extensions.Configuration** + **UserSecrets** — connection strings kept out of source
- **Spectre.Console** — colored output and interactive prompts

## Build & Run

```bash
# Restore dependencies
dotnet restore

# Run (connection strings must be in user secrets)
dotnet run

# Build only
dotnet build
```

## Configuration

Connection strings are stored in user secrets (never commit real values):

```json
{
  "ConnectionStrings": {
    "SourceAppConfig": "<source-connection-string>",
    "DestinationAppConfig": "<destination-connection-string>"
  }
}
```

Set via: `dotnet user-secrets set "ConnectionStrings:SourceAppConfig" "<value>"`

## Key Behaviours

- Labels `Development` and `Staging` are excluded from comparison.
- Values are compared after trimming whitespace — whitespace-only differences are ignored.
- Snapshot creation is optional; the tool prompts before creating one.
- The tool optionally shows keys that are identical in both stores.

## Branch & Commit Conventions

- **Feature branches:** `craig/[ticket-id]-[short-desc]`
- **Commit messages:** `FER-XXXX: Description`

## What Not to Do

- Do not commit real connection strings or secrets.
- Do not add error handling for impossible code paths.
- Do not skip trimming when comparing values — whitespace differences should be ignored.
