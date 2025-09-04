# Use official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Install Docker CLI so Jenkins can run "docker build"
RUN apt-get update && apt-get install -y docker.io && rm -rf /var/lib/apt/lists/*
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.sln .
COPY CryptoPortfolio/*.csproj ./CryptoPortfolio/
COPY CryptoPortfolio.Common/*.csproj ./CryptoPortfolio.Common/
COPY CryptoPortfolio.Infrastructure/*.csproj ./CryptoPortfolio.Infrastructure/
COPY CryptoPortfolio.Tests/*.csproj ./CryptoPortfolio.Tests/
RUN dotnet restore

# Copy the rest of the code and build
COPY . .
WORKDIR /src/CryptoPortfolio
RUN dotnet publish -c Release -o /app/publish

# Use smaller runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CryptoPortfolio.dll"]