FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["LinkManagerPro.csproj", "./"]
RUN dotnet restore "LinkManagerPro.csproj"
COPY . .
RUN dotnet build "LinkManagerPro.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LinkManagerPro.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
EXPOSE 443
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "LinkManagerPro.dll"]
