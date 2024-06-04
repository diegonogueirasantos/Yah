#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Presentation/Yah.Hub.B2W.Api/Yah.Hub.B2W.Api.csproj", "Presentation/Yah.Hub.B2W.Api/"]
COPY ["Application/Yah.Hub.Marketplace.B2W.Application/Yah.Hub.Marketplace.B2W.Application.csproj", "Application/Yah.Hub.Marketplace.B2W.Application/"]
COPY ["CrossCutting/Yah.Hub.Common/Yah.Hub.Common.csproj", "CrossCutting/Yah.Hub.Common/"]
COPY ["Presentation/Yah.Hub.Api.Application/Yah.Hub.Api.Application.csproj", "Presentation/Yah.Hub.Api.Application/"]
COPY ["Application/Yah.Hub.Marketplace.Application/Yah.Hub.Marketplace.Application.csproj", "Application/Yah.Hub.Marketplace.Application/"]
COPY ["CrossCutting/Yah.Hub.Application/Yah.Hub.Application.csproj", "CrossCutting/Yah.Hub.Application/"]
COPY ["CrossCutting/Yah.Hub.Data/Yah.Hub.Data.csproj", "CrossCutting/Yah.Hub.Data/"]
COPY ["CrossCutting/Yah.Hub.Domain/Yah.Hub.Domain.csproj", "CrossCutting/Yah.Hub.Domain/"]
RUN dotnet restore "Presentation/Yah.Hub.B2W.Api/Yah.Hub.B2W.Api.csproj"
COPY . .
WORKDIR "/src/Presentation/Yah.Hub.B2W.Api"
RUN dotnet build "Yah.Hub.B2W.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Yah.Hub.B2W.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Yah.Hub.B2W.Api.dll"]
