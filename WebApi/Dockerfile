# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Twitter.sln", "./"]
COPY ["WebApi/WebApi.csproj", "WebApi/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Shared/Shared.csproj", "Shared/"]

RUN dotnet restore "WebApi/WebApi.csproj"

COPY . .
WORKDIR "/src/WebApi"

RUN dotnet build "WebApi.csproj" -c Release -o /app/build --no-restore

RUN dotnet publish "WebApi.csproj" -c Release -o /app/publish --no-build

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN groupadd --system twitter \
    && useradd --system --gid twitter --create-home --home-dir /home/twitter twitter

COPY --from=build /app/publish .
RUN chown -R twitter:twitter /app

USER twitter

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "WebApi.dll"]
