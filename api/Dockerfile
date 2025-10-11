# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./*.sln ./
COPY ./PurestAdmin.Api.Host/*.csproj ./PurestAdmin.Api.Host/
COPY ./PurestAdmin.Application/*.csproj ./PurestAdmin.Application/
COPY ./PurestAdmin.BackgroundService/*.csproj ./PurestAdmin.BackgroundService/
COPY ./PurestAdmin.Core/*.csproj ./PurestAdmin.Core/
COPY ./PurestAdmin.Multiplex/*.csproj ./PurestAdmin.Multiplex/
COPY ./PurestAdmin.Multiplex.Contracts/*.csproj ./PurestAdmin.Multiplex.Contracts/
COPY ./PurestAdmin.SqlSugar/*.csproj ./PurestAdmin.SqlSugar/
COPY ./PurestAdmin.Workflow/*.csproj ./PurestAdmin.Workflow/
COPY ./PurestAdmin.Zero/*.csproj ./PurestAdmin.Zero/

# Restore dependencies
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish ./PurestAdmin.Api.Host/PurestAdmin.Api.Host.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .

# Copy container-specific appsettings
COPY ./PurestAdmin.Api.Host/appsettings.Container.json ./appsettings.Container.json

# Expose port
EXPOSE 80
EXPOSE 443

# Run the application
ENTRYPOINT ["dotnet", "PurestAdmin.Api.Host.dll", "--environment=Container"]