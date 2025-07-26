# Build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the .csproj file first (faster builds via layer caching)
COPY HOMEOWNER/HOMEOWNER.csproj HOMEOWNER/
RUN dotnet restore "HOMEOWNER/HOMEOWNER.csproj"

# Copy everything else
COPY . .
WORKDIR /src/HOMEOWNER
RUN dotnet publish "HOMEOWNER.csproj" -c Release -o /app/publish
