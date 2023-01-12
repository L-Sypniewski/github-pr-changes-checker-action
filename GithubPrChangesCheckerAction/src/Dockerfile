FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["GithubPrChangesCheckerAction/src/GithubPrChangesCheckerAction.csproj", "GithubPrChangesCheckerAction/src/"]
RUN dotnet restore "GithubPrChangesCheckerAction/src/GithubPrChangesCheckerAction.csproj"
COPY . .
WORKDIR "/src/GithubPrChangesCheckerAction/src"
RUN dotnet build "GithubPrChangesCheckerAction.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GithubPrChangesCheckerAction.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GithubPrChangesCheckerAction.dll"]
