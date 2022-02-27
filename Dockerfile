FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ENV ASPNETCORE_ENVIRONMENT=Production

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Json.More/Json.More.csproj", "Json.More/"]
COPY ["JsonLogic/JsonLogic.csproj", "JsonLogic/"]
COPY ["JsonPatch/JsonPatch.csproj", "JsonPatch/"]
COPY ["JsonPath/JsonPath.csproj", "JsonPath/"]
COPY ["JsonPointer/JsonPointer.csproj", "JsonPointer/"]
COPY ["JsonSchema/JsonSchema.csproj", "JsonSchema/"]
COPY ["TryJsonEverything/TryJsonEverything.csproj", "TryJsonEverything/"]
RUN dotnet restore "TryJsonEverything/TryJsonEverything.csproj"
COPY . .
WORKDIR "/src/TryJsonEverything"
RUN dotnet build "TryJsonEverything.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TryJsonEverything.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD ["dotnet", "TryJsonEverything.dll"]