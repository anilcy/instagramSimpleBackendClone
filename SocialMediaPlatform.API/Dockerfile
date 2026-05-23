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
COPY ["SocialMediaPlatform.API/SocialMediaPlatform.API.csproj", "SocialMediaPlatform.API/"]
COPY ["SocialMediaPlatform.Business/SocialMediaPlatform.Business.csproj", "SocialMediaPlatform.Business/"]
COPY ["SocialMediaPlatform.Data/SocialMediaPlatform.Data.csproj", "SocialMediaPlatform.Data/"]
COPY ["SocialMediaPlatform.Entities/SocialMediaPlatform.Entities.csproj", "SocialMediaPlatform.Entities/"]

# Restore dependencies
RUN dotnet restore "SocialMediaPlatform.API/SocialMediaPlatform.API.csproj"

# Copy the rest of the source code
COPY . .

# Build the app
WORKDIR "/src/SocialMediaPlatform.API"
RUN dotnet build "SocialMediaPlatform.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ===== Publish stage =====
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SocialMediaPlatform.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ===== Final image =====
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SocialMediaPlatform.API.dll"]