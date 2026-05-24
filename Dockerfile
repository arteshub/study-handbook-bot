FROM node:20-alpine AS frontend-build
WORKDIR /frontend
COPY frontend/package*.json ./
RUN npm ci
COPY frontend .
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY backend/HandbookBot.Domain/HandbookBot.Domain.csproj HandbookBot.Domain/
COPY backend/HandbookBot.Application/HandbookBot.Application.csproj HandbookBot.Application/
COPY backend/HandbookBot.Infrastructure/HandbookBot.Infrastructure.csproj HandbookBot.Infrastructure/
COPY backend/HandbookBot.API/HandbookBot.API.csproj HandbookBot.API/
RUN dotnet restore HandbookBot.API/HandbookBot.API.csproj
COPY backend/ .
RUN dotnet publish HandbookBot.API/HandbookBot.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=backend-build /app/publish .
COPY --from=frontend-build /frontend/dist ./wwwroot
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "HandbookBot.API.dll"]
