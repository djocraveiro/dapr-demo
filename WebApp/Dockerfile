FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Copy csproj and restore as distinct layers
WORKDIR /src
COPY ["WebApp.csproj", "./"]
RUN dotnet restore "./WebApp.csproj"
COPY . .

# Build and Publish
WORKDIR /src
RUN dotnet build "WebApp.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "WebApp.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApp.dll"]