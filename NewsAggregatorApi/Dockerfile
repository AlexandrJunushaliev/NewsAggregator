# Base image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Set the working directory
WORKDIR /app
COPY ./Utils ./Utils/
COPY ./Infrastructure/ ./Infrastructure/
COPY ./NewsProcessor ./NewsProcessor/
RUN rm ./NewsProcessor/appsettings.json
RUN rm ./NewsProcessor/NLog.config
# Copy the project files to the container
COPY ./NewsAggregatorApi ./NewsAggregatorApi

WORKDIR /app/NewsAggregatorApi

# Restore, build, and publish the .NET application
RUN dotnet restore ./NewsAggregatorApi.csproj
RUN dotnet build ./NewsAggregatorApi.csproj --configuration Release --no-restore
RUN dotnet publish ./NewsAggregatorApi.csproj --configuration Release --no-build --output /app/publish

# Start a new stage for the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime

# Set the working directory
WORKDIR /app/NewsAggregatorApi

# Copy the published .NET files from the build stage
COPY --from=build /app/publish .

# Expose port 80 for the application
EXPOSE 80

# Set the entry point for the container
ENTRYPOINT ["dotnet", "NewsAggregatorApi.dll"]
