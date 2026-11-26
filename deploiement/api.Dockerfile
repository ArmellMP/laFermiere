# Image de l'API .NET. Construite depuis la racine du dépôt (voir docker-compose.yml).
# Fonctionne sur processeurs Intel/AMD et Arm (serveurs Ampere d'Oracle Cloud).

# 1. Compilation
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS construction
WORKDIR /depot
COPY global.json laFermiere.slnx ./
COPY src/ src/
RUN dotnet publish src/PresentationApi/PresentationApi.csproj --configuration Release --output /publication

# 2. Exécution : image légère, sans le SDK, lancée avec un utilisateur non administrateur
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=construction /publication .
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "PresentationApi.dll"]
