# DevInsights
Developer Productivity Analytics Dashboard powered by Azure DevOps + AI.

## Features
- Commit analysis from Azure DevOps (last 90 days, configurable)
- AI classification via Microsoft Semantic Kernel + OpenAI
- Technology detection per commit (C#, React, Python, SQL, Docker, etc.)
- AI/LLM work detection (Copilot patterns, AI library usage)
- React dashboard with charts (Recharts)
- Microsoft OAuth PKCE via MSAL
- Single Docker container (.NET + React)
- EF Core SQLite with incremental sync

## Tech Stack
| Layer | Technology |
|-------|-----------|
| Frontend | React 18, TypeScript, Vite, Recharts, MSAL |
| Backend | ASP.NET Core 8, C# |
| AI | Microsoft Semantic Kernel, OpenAI GPT-4o-mini |
| Database | SQLite via EF Core |
| AzDo | Microsoft.TeamFoundationServer.Client |
| Auth | Microsoft Identity Web (JWT Bearer + PKCE) |

See [RUNME.md](RUNME.md) for setup instructions.
