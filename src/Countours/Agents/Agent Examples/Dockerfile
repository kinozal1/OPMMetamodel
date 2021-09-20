FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Countours/Agents/Agent Examples/Agent Examples.csproj", "Agent Examples/"]
RUN dotnet restore "Countours/Agents/Agent Examples/Agent Examples.csproj"
COPY . .
WORKDIR "/src/Agent Examples"
RUN dotnet build "Agent Examples.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Agent Examples.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Agent Examples.dll"]
