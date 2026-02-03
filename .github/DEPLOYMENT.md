# Deployment Setup

## GitHub Configuration

### 1. Create Environments

In your repo: **Settings → Environments** → Create:
- `staging` (for main branch)
- `production` (for release branch)

Optionally add protection rules (e.g., require approval for production).

### 2. Secrets & Variables

**Staging** (`staging` environment or repository-level):

| Name | Type | Description |
|------|------|-------------|
| `AZURE_WEBAPP_PUBLISH_PROFILE_STAGING` | Secret | Download from Azure App Service → Overview → Get publish profile |
| `AZURE_WEBAPP_NAME_STAGING` | Variable | Your staging App Service name (e.g. `warriorsguild-staging`) |

**Production** (`production` environment or repository-level):

| Name | Type | Description |
|------|------|-------------|
| `AZURE_WEBAPP_PUBLISH_PROFILE_PRODUCTION` | Secret | Download from Azure App Service → Overview → Get publish profile |
| `AZURE_WEBAPP_NAME_PRODUCTION` | Variable | Your production App Service name (e.g. `warriorsguild`) |

### 3. Azure App Services

Create two App Services in Azure (or one app with staging slot):

- **Staging:** Deploys from `main` branch
- **Production:** Deploys from `release` branch

## Branch Strategy

- `main` → deploys to **staging**
- `release` → deploys to **production** (merge from main when ready)
