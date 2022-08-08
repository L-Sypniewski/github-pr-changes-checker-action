# Set the base image as the .NET 6.0 SDK (this includes the runtime)
FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env

# Copy everything and publish the release (publish implicitly restores and builds)
WORKDIR /app
COPY . ./
RUN dotnet publish ./github-pr-changes-checker-action/github-pr-changes-checker-action.csproj -c Release -o out --no-self-contained

# Label the container
LABEL maintainer="Łukasz Sypniewski <l.sypniewski@gmail.com>"
LABEL repository="https://github.com/L-Sypniewski/github-pr-changes-checker-action"
LABEL homepage="https://github.com/L-Sypniewski/github-pr-changes-checker-action"

# Label as GitHub action
LABEL com.github.actions.name="Github PR Changes Checker"
# Limit to 160 characters
LABEL com.github.actions.description="Checks which projects have been modified in a PR"
# See branding:
# https://docs.github.com/actions/creating-actions/metadata-syntax-for-github-actions#branding
LABEL com.github.actions.icon="activity"
LABEL com.github.actions.color="orange"

# Relayer the .NET SDK, anew with the build output
FROM mcr.microsoft.com/dotnet/runtime:6.0
COPY --from=build-env /app/out .
ENTRYPOINT [ "dotnet", "/github-pr-changes-checker-action" ]