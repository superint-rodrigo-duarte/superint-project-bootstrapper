# Project Bootstrapper

Project scaffolding tool that automates Git, CI/CD, container registry, database, and deployment setup in one step.

## What It Does

Project Bootstrapper automates the full infrastructure setup for new projects:

- **Git** — Creates repositories on GitHub (or GitLab)
- **Container Registry** — Creates Harbor projects with retention policies
- **CI/CD** — Creates Jenkins folders, credentials, and pipelines with auto-generated Jenkinsfiles
- **Database** — Creates PostgreSQL users for staging and production environments
- **Docker Compose** — Generates environment-specific compose files with Traefik reverse proxy labels
- **Deployment** — Deploys compose files to remote servers via SSH/SCP

Each step is optional and independently toggleable — use only what you need.

## Supported Project Types

| Type | What Gets Created |
|------|-------------------|
| **Backend** | 1 repo, 1 pipeline, 1 container registry path, backend compose service |
| **Frontend** | 1 repo, 1 pipeline, 1 container registry path, frontend compose service |
| **Fullstack** | 2 repos, 2 pipelines, 2 container registry paths, full compose with both services |

**Supported languages:** C# (.NET) and Node.js — affects Jenkinsfile templates and Docker Compose port/environment configuration.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Access to the services you intend to use (GitHub/GitLab, Harbor, Jenkins, PostgreSQL, SSH servers)
- API tokens / credentials for those services

## Getting Started

### 1. Clone and Build

```bash
git clone https://github.com/superint-ai/superint-project-bootstrapper.git
cd superint-project-bootstrapper
dotnet build
```

### 2. Run

```bash
cd superint.ProjectBootstrapper.UI
dotnet run
```

### 3. Configure Credentials

The application has a built-in **Secrets Manager** where you can configure all service credentials directly from the UI. Alternatively, you can use configuration files or environment variables.

#### Option A: Configuration File

Create an `appsettings.local.json` file in the `superint.ProjectBootstrapper.UI` directory:

```json
{
  "Git": {
    "Provider": "github",
    "BaseUrl": "https://api.github.com",
    "ApiToken": "ghp_your_token_here",
    "DefaultNamespace": "your-org"
  },
  "ContainerRegistry": {
    "Provider": "harbor",
    "Url": "harbor.example.com",
    "Username": "admin",
    "Password": "your_password",
    "RetentionPolicy": {
      "Enabled": true,
      "RetainCount": 5,
      "RepositoryPattern": "**",
      "TagPattern": "**",
      "IncludeUntagged": true
    },
    "ProjectAdminUsers": ["admin"]
  },
  "Jenkins": {
    "BaseUrl": "https://jenkins.example.com",
    "Username": "admin",
    "ApiToken": "your_jenkins_token",
    "DefaultFolder": "your-folder",
    "MaxBuildsToKeep": 5,
    "Environments": ["staging", "main"],
    "CredentialsId": "git-credentials",
    "DefaultBranch": "main",
    "JenkinsfilePath": "Jenkinsfile"
  },
  "Servers": {
    "Stg": {
      "Host": "stg.example.com",
      "Port": 22,
      "Username": "deploy",
      "Password": "your_password"
    },
    "Prd": {
      "Host": "prd.example.com",
      "Port": 22,
      "Username": "deploy",
      "Password": "your_password"
    }
  },
  "PostgresContainer": {
    "Stg": {
      "ContainerName": "postgres",
      "AdminUser": "postgres",
      "AdminPassword": "your_db_password"
    },
    "Prd": {
      "ContainerName": "postgres",
      "AdminUser": "postgres",
      "AdminPassword": "your_db_password"
    }
  },
  "Deployment": {
    "BasePath": "/opt/project-data",
    "NetworkName": "app-network",
    "ReverseProxyHostStaging": "stg.example.com",
    "ReverseProxyHostProduction": "example.com"
  }
}
```

> `appsettings.local.json` is git-ignored and will never be committed.

#### Option B: Environment Variables

Use the `PROJINIT_` prefix with `__` as section separator:

```bash
export PROJINIT_Git__BaseUrl="https://api.github.com"
export PROJINIT_Git__ApiToken="ghp_your_token_here"
export PROJINIT_Jenkins__BaseUrl="https://jenkins.example.com"
# ... etc
```

## Application Sections

The desktop application provides four main sections:

### Dashboard

Overview of the application and quick access to all features.

### Secrets Manager

Configure and manage all service credentials (Git, Harbor, Jenkins, Servers, Database) directly within the application.

### Test Connections

Validate connectivity to all configured services before running the bootstrap process. Tests Git API access, Harbor connectivity, Jenkins API, SSH connections to staging/production servers, and PostgreSQL database access.

### Bootstrap Wizard

Step-by-step wizard to configure and execute project creation:

1. **Project Info** — Name, type (Backend/Frontend/Fullstack), description, languages
2. **Service Selection** — Choose which resources to create (repo, registry, pipeline, database, compose, deployment)
3. **Configuration** — Fine-tune settings for each selected service
4. **Review & Execute** — Confirm everything and run the bootstrap process

## Architecture

```
superint.ProjectBootstrapper.sln
│
├── Shared/                  # Enums, constants, string extensions
├── DTO/                     # Configuration models and API response types
├── Infrastructure/          # Service integrations and templates
│   ├── Services/
│   │   ├── GitHubService           # GitHub API (repo creation)
│   │   ├── HarborService           # Harbor API (registry projects, retention)
│   │   ├── JenkinsService          # Jenkins API (folders, credentials, pipelines)
│   │   ├── JenkinsfileService      # Jenkinsfile generation from templates
│   │   ├── PostgresDatabaseService # PostgreSQL user creation via SSH
│   │   ├── DockerComposeService    # Docker Compose file generation
│   │   ├── DeploymentService       # Remote deployment via SCP
│   │   └── SshService              # SSH connectivity
│   └── Templates/
│       ├── docker-compose/         # Scriban templates (backend, frontend, fullstack)
│       ├── jenkinsfile/            # Jenkinsfile templates (C#, Node.js)
│       └── jenkins/                # Jenkins XML config templates
├── Application/             # Orchestration layer
│   └── ProjectBootstrapService     # Task builder and executor
└── UI/                      # Desktop application (Avalonia + MVVM)
    ├── Views/               # AXAML views (Dashboard, Secrets, Connections, Wizard)
    └── ViewModels/          # CommunityToolkit.Mvvm view models
```

## Configuration Reference

| Section | Property | Description |
|---------|----------|-------------|
| **Git** | `Provider` | `github` or `gitlab` |
| **Git** | `BaseUrl` | Git provider API URL |
| **Git** | `ApiToken` | Personal access token |
| **Git** | `DefaultNamespace` | Default organization / group |
| **ContainerRegistry** | `Url` | Harbor registry hostname |
| **ContainerRegistry** | `Username` / `Password` | Registry credentials |
| **ContainerRegistry** | `RetentionPolicy` | Auto-cleanup settings (retain count, patterns) |
| **ContainerRegistry** | `ProjectAdminUsers` | Users granted admin on new projects |
| **Jenkins** | `BaseUrl` | Jenkins server URL |
| **Jenkins** | `Username` / `ApiToken` | Jenkins credentials |
| **Jenkins** | `DefaultFolder` | Root folder for pipelines |
| **Jenkins** | `MaxBuildsToKeep` | Build history retention (default: 5) |
| **Jenkins** | `Environments` | Pipeline environments (e.g. `["staging", "main"]`) |
| **Jenkins** | `CredentialsId` | Jenkins credentials ID for Git access |
| **Jenkins** | `DefaultBranch` | Branch to build (default: `main`) |
| **Servers** | `Stg` / `Prd` | SSH connection details (host, port, username, password) |
| **PostgresContainer** | `Stg` / `Prd` | Container name and admin credentials |
| **Deployment** | `BasePath` | Base path on remote servers |
| **Deployment** | `NetworkName` | Docker network name |
| **Deployment** | `ReverseProxyHostStaging` / `Production` | Traefik router hostnames |

### Obtaining API Tokens

**GitHub** — Settings > Developer Settings > Personal Access Tokens > Fine-grained tokens. Required scopes: `repo` (full control).

**Jenkins** — User menu > Configure > API Token > Add new Token.

**Harbor** — Use your Harbor login credentials, or create a robot account for automation.

## Generated Templates

The tool generates files using [Scriban](https://github.com/scriban/scriban) templates:

**Docker Compose** — Environment-aware compose files with:
- Container registry image references
- Traefik reverse proxy labels (routing, path prefixes, strip prefix middleware)
- Health checks
- Environment-specific variables (`ASPNETCORE_ENVIRONMENT`, `NODE_ENV`)
- External Docker network attachment

**Jenkinsfile** — CI/CD pipeline definitions with:
- Parameterized builds (`ENVIRONMENT`, `FORCE_REBUILD`)
- Docker build and push stages
- SSH-based deployment to target servers
- Variants for C# and Node.js backends, and Node.js frontends

## Tech Stack

| Technology | Purpose |
|------------|---------|
| [.NET 8](https://dotnet.microsoft.com/) | Runtime |
| [Avalonia UI 11](https://avaloniaui.net/) | Cross-platform desktop framework |
| [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) | MVVM implementation |
| [Scriban](https://github.com/scriban/scriban) | Template engine for file generation |
| [SSH.NET](https://github.com/sshnet/SSH.NET) | SSH/SCP operations |
| [Npgsql](https://www.npgsql.org/) | PostgreSQL connectivity |

## Troubleshooting

**Git connection fails**
- Verify the token has the required scopes
- Ensure the base URL doesn't have a trailing `/`

**Jenkins connection fails**
- Use an API Token, not your login password
- Verify the user has permissions to create jobs and folders

**PostgreSQL connection fails**
- Confirm the admin user has `CREATEROLE` privileges
- Check network access to the database host/container

**SSH connection fails**
- Verify host, port, and credentials
- Check that the target server allows password authentication

## License

MIT
