# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Caminho do .csproj da API (relativo à raiz). Ajuste conforme o projeto.
ARG PROJECT=src/Verx.Api/Verx.Api.csproj

COPY . .
RUN dotnet restore "$PROJECT"
RUN dotnet publish "$PROJECT" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Imagem aspnet já traz o usuário não-root "app"
USER $APP_UID

# Nome do assembly da API (igual ao nome do .csproj)
ARG ASSEMBLY=Verx.Api.dll
ENV ASSEMBLY=$ASSEMBLY
ENTRYPOINT ["sh", "-c", "exec dotnet $ASSEMBLY"]
