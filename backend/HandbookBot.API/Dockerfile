FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY HandbookBot.Domain/HandbookBot.Domain.csproj HandbookBot.Domain/
COPY HandbookBot.Application/HandbookBot.Application.csproj HandbookBot.Application/
COPY HandbookBot.Infrastructure/HandbookBot.Infrastructure.csproj HandbookBot.Infrastructure/
COPY HandbookBot.API/HandbookBot.API.csproj HandbookBot.API/

RUN dotnet restore HandbookBot.API/HandbookBot.API.csproj

COPY . .

RUN dotnet publish HandbookBot.API/HandbookBot.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "HandbookBot.API.dll"]
