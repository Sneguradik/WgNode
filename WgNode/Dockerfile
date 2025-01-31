FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

RUN apt-get update && apt-get install -y wireguard-tools iptables iproute2 

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WgNode/WgNode.csproj", "WgNode/"]
RUN dotnet restore "WgNode/WgNode.csproj"
COPY . .
WORKDIR "/src/WgNode"
RUN dotnet build "WgNode.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WgNode.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "WgNode.dll"]
