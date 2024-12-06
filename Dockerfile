FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

ARG VERSION
ARG ART_URL
ARG ART_USER
ARG ART_PASS

WORKDIR /app

COPY DarwinAuthorization/* .
RUN dotnet nuget add source --name crossknowledge/phoenix $ART_URL --username $ART_USER --password $ART_PASS --store-password-in-clear-text && \
    dotnet restore &&\
    dotnet build /p:Version=$VERSION DarwinAuthorization.csproj  -c Release && \
    dotnet pack /p:Version=$VERSION DarwinAuthorization.csproj -c Release && \
    dotnet nuget push ./bin/Release/DarwinAuthorization.$VERSION.nupkg --source crossknowledge/phoenix
