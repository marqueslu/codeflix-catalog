FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["src/Codeflix.Catalog.Api/Codeflix.Catalog.Api.csproj", "src/Codeflix.Catalog.Api/"]
RUN dotnet restore "src/Codeflix.Catalog.Api/Codeflix.Catalog.Api.csproj"
COPY . .
WORKDIR "/src/src/Codeflix.Catalog.Api"
RUN dotnet build "Codeflix.Catalog.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Codeflix.Catalog.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Codeflix.Catalog.Api.dll"]
