# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first to leverage Docker layer caching
COPY ["CircleApp.sln", "./"]
COPY ["CircleApp/CircleApp.csproj", "CircleApp/"]
COPY ["CircleApp.Data/CircleApp.Data.csproj", "CircleApp.Data/"]

# Restore dependencies
RUN dotnet restore "CircleApp.sln"

# Copy all remaining source files
COPY . .

# Build and publish the web application
WORKDIR "/src/CircleApp"
RUN dotnet publish "CircleApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# ASP.NET Core 8 default HTTP port
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

# Copy published output from build stage
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CircleApp.dll"]
