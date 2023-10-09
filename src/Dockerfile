FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ENV ASPNETCORE_ENVIRONMENT Lighthouse

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["./src/Lighthouse.csproj", "src/"]
RUN dotnet restore "./src/Lighthouse.csproj"
COPY . .
RUN dotnet build "./src/Lighthouse.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./src/Lighthouse.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lighthouse.dll"]
