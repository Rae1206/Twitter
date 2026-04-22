# =====================================================
# Twitter API - Dockerfile para Render
# =====================================================
# Optimizado para producción
# =====================================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar todo el codigo fuente
COPY . .

# Restaurar todos los proyectos
RUN dotnet restore

# Compilar todos los proyectos
RUN dotnet build -c Release

# Publicar WebApi
RUN dotnet publish WebApi/WebApi.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Crear usuario no-root (opcional, para producción)
RUN groupadd -r twitter && useradd -r -g twitter twitter || true

COPY --from=build /app/publish .
RUN chown -R twitter:twitter /app || true

USER twitter

# Exponer puerto
EXPOSE 8080

ENTRYPOINT ["dotnet", "WebApi.dll"]