# Set Environment

In Bash, you run: export ASPNETCORE_ENVIRONMENT=Development

# Create New Migration

dotnet-ef migrations add --project Bejebeje.DataAccess --startup-project Bejebeje.Api InitialCreate

# Update Database

dotnet-ef database update --project Bejebeje.DataAccess --startup-project Bejebeje.Api