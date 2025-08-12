# ===== Base runtime image =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ===== Build stage =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only project files for caching
COPY ["instagramClone.API/instagramClone.API.csproj", "instagramClone.API/"]
COPY ["instagramClone.Business/instagramClone.Business.csproj", "instagramClone.Business/"]
COPY ["instagramClone.Data/instagramClone.Data.csproj", "instagramClone.Data/"]
COPY ["instagramClone.Entities/instagramClone.Entities.csproj", "instagramClone.Entities/"]

# Restore dependencies
RUN dotnet restore "instagramClone.API/instagramClone.API.csproj"

# Copy the rest of the source code
COPY . .

# Build the app
WORKDIR "/src/instagramClone.API"
RUN dotnet build "instagramClone.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ===== Publish stage =====
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "instagramClone.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ===== Final image =====
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "instagramClone.API.dll"]