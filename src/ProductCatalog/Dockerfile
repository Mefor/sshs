#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#USER app
#WORKDIR /app
#EXPOSE 80
#
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["ProductCatalog/ProductCatalog.csproj", "ProductCatalog/"]
#RUN dotnet restore "./ProductCatalog/./ProductCatalog.csproj"
#COPY . .
#WORKDIR "/src/ProductCatalog"
#RUN dotnet build "./ProductCatalog.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./ProductCatalog.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "ProductCatalog.dll"]

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /app
COPY . .

RUN dotnet restore \
    ProductCatalog.csproj

RUN dotnet publish \
    --configuration Release \
    --self-contained false \
    --runtime linux-x64 \
    --output /app/publish \
    ProductCatalog.csproj

FROM mcr.microsoft.com/dotnet/aspnet:8.0 as final
USER app
WORKDIR /app
COPY --from=base /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProductCatalog.dll"]