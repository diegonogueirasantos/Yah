#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Presentation/Yah.Hub.MercadoLivre.Api/Yah.Hub.MercadoLivre.Api.csproj", "Presentation/Yah.Hub.MercadoLivre.Api/"]
COPY ["Application/Yah.Hub.Marketplace.MercadoLivre.Application/Yah.Hub.Marketplace.MercadoLivre.Application.csproj", "Application/Yah.Hub.Marketplace.MercadoLivre.Application/"]
COPY ["CrossCutting/Yah.Hub.Common/Yah.Hub.Common.csproj", "CrossCutting/Yah.Hub.Common/"]
COPY ["Application/Yah.Hub.Marketplace.Application/Yah.Hub.Marketplace.Application.csproj", "Application/Yah.Hub.Marketplace.Application/"]
COPY ["CrossCutting/Yah.Hub.Application/Yah.Hub.Application.csproj", "CrossCutting/Yah.Hub.Application/"]
COPY ["CrossCutting/Yah.Hub.Data/Yah.Hub.Data.csproj", "CrossCutting/Yah.Hub.Data/"]
COPY ["CrossCutting/Yah.Hub.Domain/Yah.Hub.Domain.csproj", "CrossCutting/Yah.Hub.Domain/"]
COPY ["Presentation/Yah.Hub.Api.Application/Yah.Hub.Api.Application.csproj", "Presentation/Yah.Hub.Api.Application/"]
RUN dotnet restore "Presentation/Yah.Hub.MercadoLivre.Api/Yah.Hub.MercadoLivre.Api.csproj"
COPY . .
WORKDIR "/src/Presentation/Yah.Hub.MercadoLivre.Api"
RUN dotnet build "Yah.Hub.MercadoLivre.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Yah.Hub.MercadoLivre.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Yah.Hub.MercadoLivre.Api.dll"]
