#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["DHI.Workflow.Host.SeaportOpx/DHI.Workflow.Host.SeaportOpx.csproj", "DHI.Workflow.Host.SeaportOpx/"]
RUN dotnet restore "DHI.Workflow.Host.SeaportOpx/DHI.Workflow.Host.SeaportOpx.csproj"
COPY . .
WORKDIR "/src/DHI.Workflow.Host.SeaportOpx"
RUN dotnet build "DHI.Workflow.Host.SeaportOpx.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DHI.Workflow.Host.SeaportOpx.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DHI.Workflow.Host.SeaportOpx.dll"]