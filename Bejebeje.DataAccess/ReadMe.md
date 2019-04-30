# Create New Migration

dotnet ef migrations --project Bejebeje.DataAccess --startup-project Bejebeje.Api add NewMigrationName

# Update Database

dotnet ef database --project Bejebeje.DataAccess --startup-project Bejebeje.Api update