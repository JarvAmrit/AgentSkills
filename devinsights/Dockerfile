FROM node:20-alpine AS frontend-build
WORKDIR /app/frontend
COPY frontend/devinsights-ui/package*.json ./
RUN npm ci
COPY frontend/devinsights-ui/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /app
COPY backend/ ./
RUN dotnet restore DevInsights.slnx
RUN dotnet publish src/DevInsights.API/DevInsights.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=backend-build /app/publish .
COPY --from=frontend-build /app/frontend/dist ./wwwroot
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "DevInsights.API.dll"]
