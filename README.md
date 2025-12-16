# mrtc

MRTC — a small .NET 10 Web API solution with unit test project.

## Summary

`MRTC.Test` provides a sample .NET 10 Web API used for demonstration and testing purposes. The repository contains an API implementation and accompanying unit tests to demonstrate some of the latest .NET capabilities. 

## Key features
- Restful Web API built with ASP.NET Core 10 that exposes Get/Post/Put/Delete endpoints for sample data from a JSON file.
- Validation using Data Annotations and IValidatableObject for Post/Put input parameters.
- Basic Authentication and Authorization is configured for Post/Put/Delete endpoints.
- Sample JSON file is updated after Post/Put/Delete operations to simulate data persistence.
- Serilog is used for logging enriched structured logs into File and Console sinks and requests are logged with Serilog middleware.
- Swagger/OpenAPI is enabled for API documentation and Postman was used for testing.
- GitHub is used as repository hosting service and branch rulesets are applied to protect main branch.
- GitHub Actions workflow for CI to build and test on pushes and pull requests.
- Code coverage reports generated during test runs and reported to SonarCloud.
- Application is built into Docker container for easy deployment.

## Project layout

- `src/Mrtc.Test.Api/` — main Web API project
  - `Program.cs` — application entrypoint, DI and middleware configuration
  - `Controllers/` — API controllers exposing endpoints (e.g. CRUD or sample endpoints)
  - `Models/` — DTOs and domain models
  - `Services/` — business logic and service abstractions/implementations
  - `appsettings.json` — configuration
- `test/Mrtc.Test.Api.UnitTest/` — unit test project
  - Tests target controllers, services and other units in the API project

This layout follows common .NET Web API conventions: controllers handle HTTP, models represent request/response payloads, and services contain business logic and data access abstractions.

## Build, run and test

Prerequisites: .NET 10 SDK

Build and test the solution:

```bash
dotnet restore
dotnet build
dotnet test
```
