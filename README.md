# github-pr-changes-checker-action

## Examples of files with secrets


### docker-compose.debug.yml
```yaml
version: '3.8'

services:
  githubprchangescheckeraction:
    build:
      args:
        BUILD_CONFIG: Debug
    entrypoint: # We have to use entrypoint instead of `command` so that Rider debugging works: https://stackoverflow.com/questions/72588085/rider-debug-docker-compose-does-not-recognize-cli-options/73292166#73292166
      [
        "dotnet",
        "/github-pr-changes-checker-action.dll",
        "-o",
        "L-Sypniewski",
        "-n",
        "My-repo-name",
        "-t",
        "ghp_MY_GITHUB_TOKEN",
        "-p",
        "120"
      ]
    volumes:
      - ~/.vsdbg:/remote_debugger:rw
```

### secrets.env
```yaml
GITHUB_TOKEN=ghp_MY_GITHUB_TOKEN
```

