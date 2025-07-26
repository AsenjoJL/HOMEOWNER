# Base image with ASP.NET Core runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy .csproj file and restore
COPY ["HOMEOWNER.csproj", "./"]
RUN dotnet restore "HOMEOWNER/HOMEOWNER.csproj"

# Copy everything else
COPY . .

WORKDIR "/src/HOMEOWNER"
RUN dotnet publish "HOMEOWNER.csproj" -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HOMEOWNER.dll"]
