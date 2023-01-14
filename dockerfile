FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base

# Label the container
LABEL maintainer="Łukasz Sypniewski <l.sypniewski@gmail.com>"
LABEL repository="https://github.com/L-Sypniewski/GithubPrChangesCheckerAction"
LABEL homepage="https://github.com/L-Sypniewski/GithubPrChangesCheckerAction"
# Label as GitHub action
LABEL com.github.actions.name="Github PR Changes Checker"
# Limit to 160 characters
LABEL com.github.actions.description="Checks which projects have been modified in a PR"
# See branding:
# https://docs.github.com/actions/creating-actions/metadata-syntax-for-github-actions#branding
LABEL com.github.actions.icon="activity"
LABEL com.github.actions.color="orange"


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

ARG BUILD_CONFIG=Release
RUN dotnet build "GithubPrChangesCheckerAction.csproj" -c ${BUILD_CONFIG} -o /app/build

FROM build AS publish
RUN dotnet publish "GithubPrChangesCheckerAction.csproj" -c ${BUILD_CONFIG} -o '/app/publish' --no-self-contained /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GithubPrChangesCheckerAction.dll"]
