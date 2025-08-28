# HabiCode API

HabiCode is a .NET 9 Web API for habit tracking and management. It provides a robust set of features for users to create, manage, and track their habits with detailed metrics and customization options.

## Features

### Authentication & Authorization
- JWT-based authentication
- User registration and login
- Role-based authorization (Member and Admin roles)
- Refresh token support

### Habit Management
- Create, read, update, and delete habits
- Support for different habit types:
  - Binary habits (done/not done)
  - Measurable habits (with specific target values)
- Habit frequency tracking:
  - Daily
  - Weekly
  - Monthly
- Habit status tracking:
  - Ongoing
  - Completed
- Support for habit archiving
- Milestone tracking
- Tags for habit categorization

### API Features
- API versioning support (v1 and v2)
- HATEOAS implementation
- Data shaping capabilities
- Sorting and filtering
- Content negotiation (JSON, XML)
- Problem Details for error responses

### Technical Features
- PostgreSQL database with Entity Framework Core
- Identity management using ASP.NET Core Identity
- FluentValidation for request validation
- OpenTelemetry integration for observability
- Structured logging
- Memory caching
- Database migrations
- Transaction support
- Custom middleware for error handling

## Project Structure

```
src/
??? HabiCode.Api/
    ??? Controllers/       # API endpoints
    ??? Database/         # DbContext and database configuration
    ??? DTOs/             # Data Transfer Objects
    ??? Entities/         # Domain models
    ??? Extensions/       # Extension methods
    ??? Middleware/       # Custom middleware components
    ??? Services/         # Business logic and services
    ??? Settings/         # Application settings and configuration
```

## Technologies

- .NET 9
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- OpenTelemetry
- FluentValidation
- ASP.NET Core Identity
- JWT Authentication
- Newtonsoft.Json

## API Versioning

The API supports content negotiation based versioning using custom media types:
- `application/vnd.habicode.v1+json`
- `application/vnd.habicode.v2+json`
- `application/vnd.habicode.hateoas.v1+json`
- `application/vnd.habicode.hateoas.v2+json`

## Database Setup

The application uses two database contexts:
1. `HabiCodeDbContext` - For habit-related data
2. `HabiCodeIdentityDbContext` - For identity and authentication data

Both contexts use PostgreSQL with snake case naming convention and separate schemas for better organization.

## Security

- JWT-based authentication
- Role-based authorization
- Secure password hashing with ASP.NET Core Identity
- HTTPS enforcement
- Refresh token rotation

## Observability

The application includes comprehensive observability features:
- OpenTelemetry integration
- HTTP client instrumentation
- ASP.NET Core instrumentation
- PostgreSQL monitoring
- Runtime metrics
- Structured logging

## Development Setup

1. Ensure you have .NET 9 SDK installed
2. Configure PostgreSQL connection string in configuration
3. Run database migrations:
   ```bash
   dotnet ef database update --context HabiCodeDbContext
   dotnet ef database update --context HabiCodeIdentityDbContext
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

## API Documentation

When running in development mode, OpenAPI documentation is available to explore the API endpoints and their capabilities.