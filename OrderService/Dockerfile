FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Copy csproj and restore as distinct layers
WORKDIR /src
COPY ["OrderService.csproj", "./"]
RUN dotnet restore "./OrderService.csproj"
COPY . .

# Build and Publish
WORKDIR /src
RUN dotnet build "OrderService.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "OrderService.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderService.dll"]