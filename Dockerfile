# Build stage — M-2: Updated from .NET 8 to .NET 9
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first (layer caching)
COPY ContentForge.sln .
COPY src/ContentForge.Domain/ContentForge.Domain.csproj src/ContentForge.Domain/
COPY src/ContentForge.Application/ContentForge.Application.csproj src/ContentForge.Application/
COPY src/ContentForge.Infrastructure/ContentForge.Infrastructure.csproj src/ContentForge.Infrastructure/
COPY src/ContentForge.Bots/ContentForge.Bots.csproj src/ContentForge.Bots/
COPY src/ContentForge.API/ContentForge.API.csproj src/ContentForge.API/

RUN dotnet restore src/ContentForge.API/ContentForge.API.csproj

# Copy everything and build
COPY . .
RUN dotnet publish src/ContentForge.API/ContentForge.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage — M-1: Run as non-root user for defense-in-depth
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user (like `USER node` in Node.js Dockerfiles)
RUN adduser --disabled-password --gecos "" appuser

COPY --from=build /app/publish .

# Create rendered-media directory owned by appuser
RUN mkdir -p /app/rendered-media && chown -R appuser:appuser /app/rendered-media

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

USER appuser
ENTRYPOINT ["dotnet", "ContentForge.API.dll"]
