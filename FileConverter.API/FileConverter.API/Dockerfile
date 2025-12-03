FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# --- DÜZELTME BURADA ---
# Linux için gerekli font ve grafik kütüphanelerini ekliyoruz
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    fontconfig \
    libfreetype6 \
    xfonts-75dpi \
    xfonts-base

# Temp klasörü izni
RUN mkdir -p /app/Temp && chmod 777 /app/Temp

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "FileConverter.API.dll"]