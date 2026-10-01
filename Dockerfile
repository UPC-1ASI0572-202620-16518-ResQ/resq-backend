FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ResQ.API/ResQ.API.csproj ResQ.API/

RUN dotnet restore ResQ.API/ResQ.API.csproj

COPY ResQ.API/ ResQ.API/

WORKDIR /src/ResQ.API

RUN dotnet publish ResQ.API.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 10000

ENV ASPNETCORE_URLS=http://0.0.0.0:10000

ENTRYPOINT ["dotnet", "ResQ.API.dll"]