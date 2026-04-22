# =====================================================
# Twitter API - Dockerfile para Render
# =====================================================
# Optimizado para producción con multi-stage build
# =====================================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar solution y restore
COPY ["Twitter.slnx", "./"]
COPY ["WebApi/WebApi.csproj", "WebApi/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Shared/Shared.csproj", "Shared/"]

RUN dotnet restore "WebApi/WebApi.csproj"

# Copiar todo el codigo
COPY . .
WORKDIR "/src/WebApi"

# Build
RUN dotnet build "WebApi.csproj" -c Release -o /app/build --no-restore

# Publish
RUN dotnet publish "WebApi.csproj" -c Release -o /app/publish --no-build

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Crear usuario no-root
RUN groupadd -r twitter && useradd -r -g twitter twitter || true

COPY --from=build /app/publish .
RUN chown -R twitter:twitter /app

USER twitter

# Exponer puerto
EXPOSE 8080

ENTRYPOINT ["dotnet", "WebApi.dll"]