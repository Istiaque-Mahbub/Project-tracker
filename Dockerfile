
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["DeveloperProjectTracker.API/DeveloperProjectTracker.API.csproj", "DeveloperProjectTracker.API/"]
COPY ["DeveloperProjectTrackerDomain/DeveloperProjectTrackerDomain.csproj", "DeveloperProjectTrackerDomain/"]

RUN dotnet restore "DeveloperProjectTracker.API/DeveloperProjectTracker.API.csproj"

COPY . .

WORKDIR /src/DeveloperProjectTracker.API

RUN dotnet publish "DeveloperProjectTracker.API.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "DeveloperProjectTracker.API.dll"]
