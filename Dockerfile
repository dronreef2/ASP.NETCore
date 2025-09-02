# Use the official .NET 8.0 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/Web/API/TutorCopiloto.csproj", "src/Web/API/"]
COPY ["src/Core/Domain/Domain.csproj", "src/Core/Domain/"]
COPY ["src/Core/Application/Application.csproj", "src/Core/Application/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]

RUN dotnet restore "src/Web/API/TutorCopiloto.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/Web/API"
RUN dotnet build "TutorCopiloto.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "TutorCopiloto.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 8.0 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create a non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser:appuser /app
USER appuser

# Copy the published app
COPY --from=publish /app/publish .

# Expose the port (Heroku will set PORT environment variable)
EXPOSE 8080

# Set the entry point with dynamic port
ENTRYPOINT ["sh", "-c", "dotnet TutorCopiloto.dll --urls=http://0.0.0.0:${PORT:-8080}"]

# Health check using dynamic port
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:${PORT:-8080}/health || exit 1
