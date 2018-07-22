# SchedulerBot

A Discord bot for scheduling events, rewritten in .NET.

## Prerequisites

- [Microsoft .NET Core 2.1.1](https://www.microsoft.com/net/download)
- [PostgreSQL 10+](https://www.postgresql.org/download/)

## Installation and Usage

1. Clone the repository.
2. Create a Discord Developer App with an app bot user at https://discordapp.com/developers/applications/me, and note the app bot user token to place in the configuration file.
3. Ensure that a local PostgreSQL instance is running, and create a database called `schedulerbot`.
4. Create an `appsettings.Development.json` file in `SchedulerBot/SchedulerBot.Client` with the following contents:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  },
  "ConnectionStrings": {
    "SchedulerBotContext": "Server=localhost;Database=schedulerbot;"
  },
  "Bot": {
    "Token": "YOUR_BOT_TOKEN_HERE",
    "Prefixes": ["+"]
  }
}
```

5. `cd SchedulerBot/SchedulerBot.Data && dotnet ef database update -s ../SchedulerBot.Client/SchedulerBot.Client.csproj`
6. Build and run the `SchedulerBot.Client` project using Visual Studio or Visual Studio Code.
