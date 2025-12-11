# Use the official .NET 6.0 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["TutoringApp/TutoringApp.csproj", "TutoringApp/"]
RUN dotnet restore "TutoringApp/TutoringApp.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/TutoringApp"

# Build the application
RUN dotnet build "TutoringApp.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "TutoringApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TutoringApp.dll"]
