FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Copy csproj and restore as distinct layers
WORKDIR /src
COPY ["ProductApi.csproj", "./"]
RUN dotnet restore "./ProductApi.csproj"
COPY . .

# Build and Publish
WORKDIR /src
RUN dotnet build "ProductApi.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "ProductApi.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductApi.dll"]