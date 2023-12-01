FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./AIHub.csproj ./AIHub.csproj
RUN dotnet restore ./AIHub.csproj

# Copy everything else and build website
COPY ./ ./app/
WORKDIR /src/app/
RUN dotnet publish -c release

# Final stage / image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY --from=build /src/app/bin/Release/net8.0/publish ./
ENTRYPOINT ["dotnet", "AIHub.dll"]