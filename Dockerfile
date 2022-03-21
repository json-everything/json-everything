# See https://chrissainty.com/containerising-blazor-applications-with-docker-containerising-a-blazor-webassembly-app/
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Json.More/Json.More.csproj", "Json.More/"]
COPY ["JsonLogic/JsonLogic.csproj", "JsonLogic/"]
COPY ["JsonPatch/JsonPatch.csproj", "JsonPatch/"]
COPY ["JsonPath/JsonPath.csproj", "JsonPath/"]
COPY ["JsonPointer/JsonPointer.csproj", "JsonPointer/"]
COPY ["JsonSchema/JsonSchema.csproj", "JsonSchema/"]
COPY ["JsonSchema.Data/JsonSchema.Data.csproj", "JsonSchema.Data/"]
COPY ["JsonSchema.DataGeneration/JsonSchema.DataGeneration.csproj", "JsonSchema.DataGeneration/"]
COPY ["JsonSchema.Generation/JsonSchema.Generation.csproj", "JsonSchema.Generation/"]
COPY ["JsonSchema.UniqueKeys/JsonSchema.UniqueKeys.csproj", "JsonSchema.UniqueKeys/"]
COPY ["json-everything.net/json-everything.net.csproj", "json-everything.net/"]
RUN dotnet restore "json-everything.net/json-everything.net.csproj"
COPY . .
WORKDIR "/src/json-everything.net"
RUN dotnet build "json-everything.net.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "json-everything.net.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY nginx.conf /etc/nginx/nginx.conf