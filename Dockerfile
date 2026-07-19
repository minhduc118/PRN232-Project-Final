FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["SportCourtManagent_Server/SportCourtManagent_Server/SportCourtManagent_Server.csproj", "SportCourtManagent_Server/SportCourtManagent_Server/"]
RUN dotnet restore "SportCourtManagent_Server/SportCourtManagent_Server/SportCourtManagent_Server.csproj"

# Copy the rest of the source code
COPY . .

WORKDIR "/src/SportCourtManagent_Server/SportCourtManagent_Server"
RUN dotnet build "SportCourtManagent_Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SportCourtManagent_Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SportCourtManagent_Server.dll"]
