FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AuthorizationServer/AuthorizationServer.csproj", "AuthorizationServer/"]
COPY ["AuthorizationServer.Test/AuthorizationServer.Test.csproj", "AuthorizationServer.Test/"]
COPY ["AuthorizationServer.sln", "."]
RUN dotnet restore "AuthorizationServer.sln"

COPY . .
WORKDIR "/src/AuthorizationServer"
RUN dotnet build "AuthorizationServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthorizationServer.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

COPY AuthorizationServer/wait-for-it.sh /app/wait-for-it.sh
RUN chmod +x /app/wait-for-it.sh

EXPOSE 5000

ENTRYPOINT ["/app/wait-for-it.sh", "authserver_postgres:5432", "--", "dotnet", "AuthorizationServer.dll"]