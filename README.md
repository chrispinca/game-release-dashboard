# Game Release Dashboard

Game Release Dashboard is a small internal tool for tracking game and software releases across environments like Dev, QA, Staging, and Prod. The project uses a React frontend, an ASP.NET Core Web API backend, Docker support, GitHub Actions CI/CD, and a Linux Azure App Service container deployment path with Azure SQL for hosted environments.

## Features

- View all releases in a dashboard table
- Add new releases with deployment notes
- Update release status from Pending to Success or Failed
- Delete old or test releases
- Filter by environment and status
- View summary cards for release health
- Check backend health with `GET /health`

## Tech stack

- React + Vite
- ASP.NET Core Web API
- Azure SQL for hosted environments
- SQLite fallback for local development
- Docker + Docker Compose
- GitHub Actions CI
- GitHub Actions CD for Azure App Service
- xUnit integration tests for the API

## Local development

### Backend

```bash
cd backend
dotnet run
```

The API runs on `http://localhost:5133` in development.

By default, local development uses a SQLite file. If `ConnectionStrings__ReleaseDb` is set, the app switches to SQL Server / Azure SQL automatically.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

The frontend runs on `http://localhost:5173` and talks to the backend at `http://localhost:5133`.

## Production hosting model

For Azure App Service container deployment, the root [Dockerfile](/Users/cpinca/Desktop/GitHub/game-release-dashboard/Dockerfile) builds the React app, copies it into `backend/wwwroot`, publishes the ASP.NET app, and ships one Linux container that serves both the dashboard UI and the API.

### Integration tests

```bash
dotnet test backend.Tests/backend.Tests.csproj
```

## API endpoints

```text
GET    /api/releases
GET    /api/releases/{id}
POST   /api/releases
PUT    /api/releases/{id}
DELETE /api/releases/{id}
GET    /health
```

## Docker

Run the full stack with:

```bash
docker compose up --build
```

Then open:

- Frontend: `http://localhost:8080`
- Backend API: `http://localhost:8081/api/releases`
- Health check: `http://localhost:8081/health`

## GitHub Actions

- `.github/workflows/ci.yml` runs build, test, and production-container validation on pushes and pull requests.
- `.github/workflows/deploy-azure-app-service.yml` builds the production image, pushes it to Azure Container Registry, and deploys that image to Azure App Service on `main`.

## Azure App Service setup

1. Create an Azure Container Registry.
2. Create an Azure App Service web app with:
   - `Publish: Container`
   - `Operating System: Linux`
3. Create an Azure SQL database.
4. In the App Service `Deployment Center` or `Container` settings, point the app at your Azure Container Registry once so the app stores the registry access configuration.
5. Add these GitHub repository secrets:
   - `AZURE_WEBAPP_NAME`
   - `AZURE_WEBAPP_PUBLISH_PROFILE`
   - `AZURE_CONTAINER_REGISTRY_LOGIN_SERVER`
   - `AZURE_CONTAINER_REGISTRY_USERNAME`
   - `AZURE_CONTAINER_REGISTRY_PASSWORD`
6. In Azure App Service Configuration, add:
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `WEBSITES_PORT=8080`
   - `ConnectionStrings__ReleaseDb=Server=tcp:<server>.database.windows.net,1433;Initial Catalog=<database>;Persist Security Info=False;User ID=<user>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`
7. Push to `main` or run the deploy workflow manually.

### Azure SQL note

Azure SQL is a stronger production story than SQLite because it supports managed backups, easier scaling, and multi-instance hosting. This repo still falls back to SQLite locally so development stays lightweight.

## GitHub push checklist

1. Push `main`.
2. Add the Azure deployment secrets.
3. Set `WEBSITES_PORT=8080` and the Azure SQL connection string in Azure App Service.
4. Let GitHub Actions build, push, and deploy the Linux container from `main`.

## Example release payload

```json
{
  "gameName": "Battlefield",
  "teamName": "Core Services",
  "buildVersion": "1.0.4",
  "environment": "QA",
  "status": "Pending",
  "releaseNotes": "Testing matchmaking service changes"
}
```
