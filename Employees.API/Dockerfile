FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies.
# This step is cached by Docker for faster builds.
COPY ["Employees.API.csproj", "./"]
RUN dotnet restore "Employees.API.csproj"

# Copy the rest of the application code
COPY . .

# Publish the application for production
RUN dotnet publish "Employees.API.csproj" -c Release -o /app/publish

# Use the final base image for the final, smaller runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# The application entry point
ENTRYPOINT ["dotnet", "Employees.API.dll"]
