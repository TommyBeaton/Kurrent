FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ARG ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["./src/Kurrent.csproj", "src/"]
RUN dotnet restore "./src/Kurrent.csproj"
COPY . .
RUN dotnet build "./src/Kurrent.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./src/Kurrent.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kurrent.dll"]
