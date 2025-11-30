# Stage 1: Build Angular app
FROM node:18-alpine AS angular-build
WORKDIR /app
# Copy package files
COPY client/package*.json ./client/
WORKDIR /app/client
# Use --legacy-peer-deps to handle peer dependency conflicts
RUN npm ci --legacy-peer-deps
# Copy Angular source
COPY client/ ./
# Build Angular (outputs to ../API/wwwroot relative to client dir)
RUN npm run build -- --configuration production

# Stage 2: Build .NET API
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app
EXPOSE 8080

# Copy csproj and restore as distinct layers
COPY API/*.csproj ./
RUN dotnet restore

# Copy API source
COPY API/ ./
# Copy Angular build output to wwwroot
# Angular build outputs to ../API/wwwroot from /app/client, so it's at /app/API/wwwroot
COPY --from=angular-build /app/API/wwwroot ./wwwroot
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT [ "dotnet", "API.dll" ]

