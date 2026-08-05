# Database initialization

The SQL used to provision the application schema is stored separately in [initialize_database.sql](src/UrlShortener.Infrastructure/Data/Scripts/initialize_database.sql).

## Why it is separate

Keeping DDL out of `Program.cs` makes the database schema easy to review, run manually in the Supabase SQL Editor, and change without mixing SQL with HTTP endpoint configuration.

## How startup works

1. The API builds `initialize_database.sql` into the Infrastructure assembly as an embedded resource.
2. `DatabaseInitializer.InitializeAsync` reads that resource at startup.
3. EF Core runs the complete script with `ExecuteSqlRawAsync`.
4. `IF NOT EXISTS` makes it safe to run again: it creates `shortened_urls` and both indexes only when absent.

## Why this project does not use EF Core migrations

This project intentionally uses a single idempotent SQL initialization script instead of EF Core migration files. It is a small assignment with one stable table, and the script can initialize an empty Supabase database as well as safely run against an existing one.

The trade-off is that schema history is not recorded in EF Core's `__EFMigrationsHistory` table. For a production system with frequent schema changes, replace this script with versioned EF Core migrations and apply them through the deployment pipeline.

## Manual use in Supabase

If automatic initialization cannot run, open Supabase **SQL Editor**, paste the contents of `initialize_database.sql`, and run it. The script creates:

- `shortened_urls`
- a unique index on `short_code`
- an index on `original_url`

The script has no credentials and is safe to commit.