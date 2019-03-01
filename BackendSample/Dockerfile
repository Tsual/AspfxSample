FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["../BackendSample/BackendSample.csproj", "../BackendSample/"]
RUN dotnet restore "../BackendSample/BackendSample.csproj"
COPY . .
WORKDIR "/src/../BackendSample"
RUN dotnet build "BackendSample.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "BackendSample.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BackendSample.dll"]