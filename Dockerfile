FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 81

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

RUN apt-get update
RUN apt-get install libidn12
RUN useradd -u 999 -m -s /bin/bash psaadm

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AutoTf.AdminPanel/AutoTf.AdminPanel.csproj", "AutoTf.AdminPanel/"]
RUN dotnet restore "AutoTf.AdminPanel/AutoTf.AdminPanel.csproj"
COPY . .

WORKDIR "/src/AutoTf.AdminPanel"
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AutoTf.AdminPanel.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AutoTf.AdminPanel.dll"]
