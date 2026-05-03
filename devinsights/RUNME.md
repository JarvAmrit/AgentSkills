# DevInsights – Step-by-Step Run Guide

## Prerequisites

Make sure the following tools are installed on your machine before you begin:

| Tool | Version | Download |
|------|---------|----------|
| .NET SDK | 8.0+ | https://dotnet.microsoft.com/download |
| Node.js | 20+ | https://nodejs.org |
| Docker Desktop | Latest | https://www.docker.com/products/docker-desktop (Docker run only) |

You will also need:
- **Azure AD App Registrations** – two registrations: one for the API (backend) and one for the SPA (frontend)
- **Azure DevOps Personal Access Token (PAT)** – with Read access to Code and Git repositories
- **OpenAI API Key** – for AI commit classification (GPT-4o-mini)

---

## Option A – Run Locally (Backend + Frontend separately)

### Step 1 – Configure the Backend

1. Open `backend/src/DevInsights.API/appsettings.json` in a text editor.
2. Replace every placeholder value with your real credentials:

```json
{
  "AzureAd": {
    "TenantId": "<your-azure-tenant-id>",
    "ClientId": "<your-API-app-registration-client-id>",
    "Audience": "<your-API-app-registration-client-id>"
  },
  "AzureDevOps": {
    "OrganizationUrl": "https://dev.azure.com/<your-org>",
    "PersonalAccessToken": "<your-pat-token>",
    "Repositories": [
      {
        "Organization": "<your-org>",
        "Project": "<your-project>",
        "RepoName": "<your-repo-name>",
        "Branches": "develop,main"
      }
    ]
  },
  "OpenAI": {
    "ApiKey": "<your-openai-api-key>",
    "ModelId": "gpt-4o-mini"
  }
}
```

> **Note:** If `OpenAI:ApiKey` is left as the placeholder, the backend will start but AI classification will fall back to heuristics. All other credentials are required.

### Step 2 – Start the Backend

```bash
cd devinsights/backend
dotnet run --project src/DevInsights.API
```

The API will start on **http://localhost:5000** (or **https://localhost:5001**).  
The Swagger UI is available at **http://localhost:5000/swagger** while running in Development mode.  
The SQLite database (`devinsights.db`) is created and migrated automatically on first startup.

### Step 3 – Configure the Frontend

```bash
cd devinsights/frontend/devinsights-ui
cp .env.example .env
```

Open the newly created `.env` file and fill in the values:

```env
VITE_AZURE_AD_CLIENT_ID=<your-SPA-app-registration-client-id>
VITE_AZURE_AD_TENANT_ID=<your-azure-tenant-id>
VITE_API_BASE_URL=http://localhost:5000
VITE_REDIRECT_URI=http://localhost:5173
```

### Step 4 – Install Frontend Dependencies

```bash
npm install
```

### Step 5 – Start the Frontend Dev Server

```bash
npm run dev
```

The React app will be available at **http://localhost:5173**.  
Open that URL in your browser and sign in with your Microsoft account.

---

## Option B – Run with Docker (Single Container)

This option builds and runs both the frontend and backend inside a single Docker container.

### Step 1 – Create the Environment File

From the `devinsights/` directory:

```bash
cd devinsights
cp frontend/devinsights-ui/.env.example .env
```

Open `.env` and set the following variables:

```env
AZDO_PAT=<your-azure-devops-pat-token>
AZURE_TENANT_ID=<your-azure-tenant-id>
AZURE_CLIENT_ID=<your-API-app-registration-client-id>
OPENAI_API_KEY=<your-openai-api-key>
```

### Step 2 – Build and Start the Container

```bash
docker-compose up --build
```

This command will:
1. Build the React frontend (`npm ci && npm run build`)
2. Build and publish the .NET 8 backend (`dotnet publish`)
3. Assemble a single ASP.NET Core container that serves both the API and the React SPA

### Step 3 – Access the App

Once the container is running, open **http://localhost:8080** in your browser.

To stop the container press `Ctrl+C`, or run:

```bash
docker-compose down
```

Data is persisted in a Docker named volume (`devinsights-data`) so commit analysis results survive container restarts.

---

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| `dotnet: command not found` | Install .NET 8 SDK and ensure it is on your PATH |
| `npm: command not found` | Install Node.js 20+ |
| Backend starts but returns 401 on every request | Double-check `AzureAd:TenantId` and `AzureAd:ClientId` in `appsettings.json` |
| Frontend shows MSAL login loop | Ensure `VITE_AZURE_AD_CLIENT_ID` and `VITE_REDIRECT_URI` match your SPA app registration |
| AI features return generic results | The `OpenAI:ApiKey` is missing or invalid; AI falls back to heuristics |
| Docker build fails at `npm ci` | Ensure Docker Desktop has internet access to download npm packages |
| Docker build fails at `dotnet restore` | Ensure Docker Desktop has internet access to download NuGet packages |
