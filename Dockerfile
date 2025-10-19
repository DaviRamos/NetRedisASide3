FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5026

ENV ASPNETCORE_URLS=http://+:5026

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["NetRedisASide3/NetRedisASide3.csproj", "NetRedisASide3/"]
RUN dotnet restore "NetRedisASide3/NetRedisASide3.csproj"
COPY . .
WORKDIR "/src/NetRedisASide3"
RUN dotnet build "NetRedisASide3.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "NetRedisASide3.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetRedisASide3.dll"]
