# SchedulerBot Database Migration Tool (v1.0.x to v2.0)

A helper tool for migrating data stored on a MongoDB database for SchedulerBot v1.0.x to a PostgreSQL database for v2.0.

## Usage

1. Make sure you have run the `dotnet ef database update` command to migrate the data model schema to the PostgreSQL database.
2. Create an `appsettings.Development.json` file and provide connection strings for the MongoDB and PostgreSQL instances.

```json
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://localhost:27017/schedulerbot",
    "SchedulerBotContext": "Server=localhost;Database=schedulerbot"
  }
}
```

3. Build and run the project.

```bash
$ dotnet build
$ cd bin/Debug/netcoreapp2.1
$ ASPNETCORE_ENVIRONMENT=Development dotnet SchedulerBot.MigrationTool.dll
```

4. Due to the requirement of case-sensitive timezones in v2.0, the migration tool will occasionally prompt to enter a timezone when encountering timezones that are not properly cased from the MongoDB database. Simply input a suitable timezone name, press Enter/Return, and the program will proceed.

```
Found approximately 1032 calendar documents.
Migrating...
93 out of 1032 calendars migrated.
Timezone Europe/oslo not found for calendar XXXXXXXXXXXXXXXXXXX.
Enter a suitable timezone: Europe/Oslo
```

5. Ensure that the program finishes running for the data migration to complete, as changes to the PostgreSQL database are only saved at the end of the process.