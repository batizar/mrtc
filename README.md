[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=batizar_mrtc&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=batizar_mrtc)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=batizar_mrtc&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=batizar_mrtc)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=batizar_mrtc&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=batizar_mrtc)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=batizar_mrtc&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=batizar_mrtc)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=batizar_mrtc&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=batizar_mrtc)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=batizar_mrtc&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=batizar_mrtc)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=batizar_mrtc&metric=coverage)](https://sonarcloud.io/summary/new_code?id=batizar_mrtc)

# mrtc

a small .NET 10 Web API solution with unit test project.

## Summary

`MRTC.Test` provides a sample .NET 10 Web API used for demonstration and testing purposes. The repository contains an API implementation and accompanying unit tests to demonstrate some of the latest .NET capabilities. 

## Key features
- Restful Web API built with ASP.NET Core 10 that exposes Get/Post/Put/Delete endpoints for sample data from a JSON file.
- Validation using Data Annotations and IValidatableObject for Post/Put input parameters.
- Basic Authentication and Authorization is configured for Post/Put/Delete endpoints.
- Sample JSON file is updated after Post/Put/Delete operations to simulate data persistence.
- Serilog is used for logging requests and enriched structured logs into File and Console sinks.
- Swagger/OpenAPI is enabled for API documentation and Postman was used for testing.
- GitHub is used as repository hosting service and branch rulesets are applied to protect main branch.
- GitHub Actions workflow for CI to build and test on pushes and pull requests.
- Code coverage reports generated during test runs and reported to [SonarCloud](https://sonarcloud.io/summary/overall?id=batizar_mrtc).
- SonarCloud, CodeQL, Secret Scanning Analysis need to pass before pull request merges. 
- Application is built into Docker container for easy deployment.

## Project layout

- `src/Mrtc.Test.Api/` — main Web API project
  - `Program.cs` — application entrypoint, DI and middleware configuration
  - `Controllers/` — API controllers exposing endpoints (e.g. CRUD or sample endpoints)
  - `Models/` — DTOs and domain models
  - `Services/` — business logic and service abstractions/implementations
  - `Handlers/` — Basic Authentication handler code 
  - `appsettings.json` — configurations
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
