FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MockServices/MockServices.csproj", "MockServices/"]
RUN dotnet restore "MockServices/MockServices.csproj"
COPY . .
WORKDIR "/src/MockServices"
RUN dotnet build "MockServices.csproj" -c Release -o /app/build
RUN dotnet publish "MockServices.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MockServices.dll"]
