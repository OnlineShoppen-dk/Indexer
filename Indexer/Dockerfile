# Use the official .NET SDK image from Microsoft
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory in the container
WORKDIR /app

# Copy the project files into the container
COPY . ./

# Build the application inside the container
RUN dotnet publish -c Release -o out

# Start a new stage from a lightweight base image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime

# Set the working directory in the container
WORKDIR /app

# Copy the published application from the build stage into the runtime stage
COPY --from=build /app/out ./

# Set the entry point for the container
ENTRYPOINT ["dotnet", "Indexer.dll"]
