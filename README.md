# github-pr-changes-checker-action

## Examples of files with secrets


### docker-compose.debug.yml
```yaml
version: '3.8'

services:
  githubprchangescheckeraction:
    command:
      - "-p 123"
      - "-n My-Repo"
      - "-o L-Sypniewski"
      - "-t ghp_MY_GITHUB_TOKEN"
    volumes:
      - ~/.vsdbg:/remote_debugger:rw

```

### secrets.env
```yaml
GITHUB_TOKEN=ghp_MY_GITHUB_TOKEN
```

