#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

RUN mkdir -p /app/Assets

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PM_Case_Managemnt_API/PM_Case_Managemnt_API.csproj", "PM_Case_Managemnt_API/"]
COPY ["PM_Case_Managemnt_Implementation/PM_Case_Managemnt_Implementation.csproj", "PM_Case_Managemnt_Implementation/"]
COPY ["PM_Case_Managemnt_Infrustructure/PM_Case_Managemnt_Infrustructure.csproj", "PM_Case_Managemnt_Infrustructure/"]
RUN dotnet restore "./PM_Case_Managemnt_API/./PM_Case_Managemnt_API.csproj"
COPY . .
WORKDIR "/src/PM_Case_Managemnt_API"
RUN dotnet build "./PM_Case_Managemnt_API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PM_Case_Managemnt_API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PM_Case_Managemnt_API.dll"]