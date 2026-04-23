# DevInsights – Quick Start

## Prerequisites
- .NET 8 SDK, Node.js 20+, Docker Desktop
- Azure AD App Registrations (API + SPA)
- Azure DevOps PAT token
- OpenAI API key

## Local Run

### Backend
1. Edit `backend/src/DevInsights.API/appsettings.json` with your credentials
2. `cd backend && dotnet run --project src/DevInsights.API`

### Frontend
1. `cd frontend/devinsights-ui && cp .env.example .env` and fill in values
2. `npm install && npm run dev`

## Docker
```bash
cp .env.example .env  # fill in vars
docker-compose up --build
```
