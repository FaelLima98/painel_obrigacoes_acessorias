# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia a solução e os csproj primeiro para aproveitar o cache de restore
COPY EAuditoria.slnx ./
COPY src/Domain/EAuditoria.Domain.csproj src/Domain/
COPY src/Application/EAuditoria.Application.csproj src/Application/
COPY src/Infrastructure/EAuditoria.Infrastructure.csproj src/Infrastructure/
COPY src/API/EAuditoria.API.csproj src/API/
COPY tests/EAuditoria.Tests/EAuditoria.Tests.csproj tests/EAuditoria.Tests/

RUN dotnet restore src/API/EAuditoria.API.csproj

# Copia o restante do código e publica
COPY . .
RUN dotnet publish src/API/EAuditoria.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080

ENTRYPOINT ["dotnet", "EAuditoria.API.dll"]
