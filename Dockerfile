FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["HabitScheduler/HabitScheduler.csproj", "HabitScheduler/"]
RUN dotnet restore "HabitScheduler/HabitScheduler.csproj"

COPY . .
WORKDIR /src/HabitScheduler
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HabitScheduler.dll"]
