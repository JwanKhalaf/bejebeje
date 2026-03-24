# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Context

Bejebeje is a community driven archive of Kurdish lyrics. It is a website where users can find Kurdish lyrics to for their favourite artists and songs. Here are the things a user can do on Bejebeje:

1. Search for a song by name to read its lyrics
2. Search for an artist by name to see all their lyrics that exist on Bejebeje
3. Sign up for an account
4. Log into Bejebeje
5. Once logged in, they can 'Like' or 'Heart' a lyric they like
6. They can add new artists (all new submissions require approval by the site admins)
7. They can add new lyrics to existing artists (again, this requires approval by the site admins)
8. They can also see who the lyrics were written by (often a poet), these are known as 'Authors' in Bejebeje
9. They can report a lyric (e.g. for errors, wrong attribution) — reports are reviewed by admins and can be Acknowledged or Dismissed

## Common Development Commands

### Building and Running
- **Build entire solution**: `dotnet build`
- **Run the web application**: `dotnet run --project Bejebeje.Mvc`
- **Run with Docker**: `docker-compose up` (requires `variables.env` file)

### Testing
- **Run all tests**: `dotnet test`
- **Run specific test project**: `dotnet test Bejebeje.Mvc.Tests` or `dotnet test Bejebeje.Services.Tests`
- **List all tests**: `dotnet test --list-tests`
- **Run tests with detailed output**: `dotnet test --verbosity detailed`

### Database Operations
- **Set environment for migrations**: `export ASPNETCORE_ENVIRONMENT=Development`
- **Create new migration**: `dotnet-ef migrations add --project Bejebeje.DataAccess --startup-project Bejebeje.Mvc [MigrationName]`
- **Update database**: `dotnet-ef database update --project Bejebeje.DataAccess --startup-project Bejebeje.Mvc`

### Frontend Assets
- **Install Tailwind dependencies**: `npm install`
- Uses Tailwind CSS 4.0.3 for styling

## Architecture Overview

### Project Structure
This is a **Clean Architecture** .NET 9.0 solution with the following layers:

- **Bejebeje.Domain**: Core domain entities (Artist, Lyric, Author) with interfaces
- **Bejebeje.DataAccess**: Entity Framework Core data layer with PostgreSQL
- **Bejebeje.Services**: Business logic and service implementations
- **Bejebeje.Models**: DTOs and ViewModels for data transfer
- **Bejebeje.Common**: Shared utilities, extensions, and exceptions
- **Bejebeje.Mvc**: ASP.NET Core MVC web application layer

### Core Domain Entities
- **Artist**: Musicians/performers with slugs, lyrics, images, and approval workflow
- **Lyric**: Song lyrics belonging to artists, with optional authors and approval system
- **Author**: Lyric writers/composers separate from performing artists
- All entities implement `IBaseEntity` (audit fields) and `IApprovable` (moderation workflow)

### Key Technologies
- **.NET 9.0** with ASP.NET Core MVC
- **Entity Framework Core** with PostgreSQL and snake_case naming
- **AWS Cognito** for authentication/authorization
- **Tailwind CSS 4.0.3** for styling
- **Docker** support with PostgreSQL container
- **Sentry** for error monitoring
- **NUnit + FluentAssertions + Moq** for testing

### Authentication & Security
- Uses AWS Cognito with OpenID Connect
- Claims-based authorization with roles
- User secrets for sensitive configuration
- Environment-based configuration (Development/Production)

### Data Flow Pattern
Controllers → Services → Data Access → Database
- Controllers handle HTTP requests and return views/JSON
- Services contain business logic and orchestrate data operations
- Repository pattern through Entity Framework DbContext
- Slug-based URLs for SEO (artists and lyrics have slugs)

### Key Service Dependencies
- `IArtistsService`: Artist management and CRUD operations
- `ILyricsService`: Lyric management with artist associations
- `ICognitoService`: User authentication and management
- `IImagesService`: Artist image handling and URL building
- `ISitemapService`: SEO sitemap generation

### Configuration Requirements
- **User Secrets** (for local development) or **variables.env** (for Docker)
- Required settings: ConnectionString, Cognito configuration, AWS credentials
- Environment-specific appsettings (Development.json, Production)

### Testing Notes
- **Current State**: Bejebeje.Mvc.Tests has active tests, Bejebeje.Services.Tests are commented out
- Uses NUnit 4.2.2 with FluentAssertions and Moq
- Database testing uses in-memory or test database setup
- Test assets stored in `/Assets/` folders