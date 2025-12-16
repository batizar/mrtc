# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

# Create a non-root user and group to run the app
ARG APP_USER=app
ARG APP_UID=1000
ARG APP_GID=1000
RUN groupadd -g ${APP_GID} ${APP_USER} \
    && useradd -u ${APP_UID} -g ${APP_GID} -s /sbin/nologin -M ${APP_USER} || true

WORKDIR /app
RUN mkdir -p /app && chown ${APP_USER}:${APP_USER} /app
USER ${APP_USER}

EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Mrtc.Test.Api/Mrtc.Test.Api.csproj", "Mrtc.Test.Api/"]
RUN dotnet restore "./Mrtc.Test.Api/Mrtc.Test.Api.csproj"
# Copy only the service project files instead of the entire context to avoid leaking sensitive files
COPY ["Mrtc.Test.Api/", "Mrtc.Test.Api/"]
WORKDIR "/src/Mrtc.Test.Api"
RUN dotnet build "./Mrtc.Test.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Mrtc.Test.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
ARG APP_USER=app
WORKDIR /app
# ensure files copied into the image are owned by the non-root user
COPY --from=publish --chown=${APP_USER}:${APP_USER} /app/publish .
# Explicitly run as non-root user in final stage
USER ${APP_USER}
ENTRYPOINT ["dotnet", "Mrtc.Test.Api.dll"]
