# Game Release Dashboard

Game Release Dashboard is a small internal tool for tracking game and software releases across environments like Dev, QA, Staging, and Prod. The project uses a React frontend, an ASP.NET Core Web API backend, Docker support, GitHub Actions CI/CD, and an Azure App Service deployment path with Azure SQL for hosted environments.

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

For Azure App Service deployment, the React app is built and copied into `backend/wwwroot` during the GitHub Actions workflow. That lets a single ASP.NET Core app serve both the API and the dashboard UI from the same App Service.

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

- `.github/workflows/ci.yml` runs build, test, and Docker validation on pushes and pull requests.
- `.github/workflows/deploy-azure-app-service.yml` builds the frontend and backend, packages the app, and deploys from `main` to Azure App Service.

## Azure App Service setup

1. Create an Azure App Service web app.
2. Create an Azure SQL database.
3. Add these GitHub repository secrets:
   - `AZURE_WEBAPP_NAME`
   - `AZURE_WEBAPP_PUBLISH_PROFILE`
4. In Azure App Service Configuration, add:
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `ConnectionStrings__ReleaseDb=Server=tcp:<server>.database.windows.net,1433;Initial Catalog=<database>;Persist Security Info=False;User ID=<user>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`
5. Push to `main` or run the deploy workflow manually.

### Azure SQL note

Azure SQL is a stronger production story than SQLite because it supports managed backups, easier scaling, and multi-instance hosting. This repo still falls back to SQLite locally so development stays lightweight.

## GitHub push checklist

1. Push `main`.
2. Add the Azure deployment secrets.
3. Set the Azure SQL connection string in Azure App Service.
4. Let GitHub Actions handle CI and production deploys from `main`.

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
