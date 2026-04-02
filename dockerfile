FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# 1. Crear carpeta exclusiva para la BD y asegurar que el usuario 'app' tenga permisos
RUN mkdir /app/data && chown app:app /app/data

# 2. Copiar los archivos con el dueño correcto (app)
COPY --chown=app:app --from=build /app/out .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
# 3. Configuramos dónde se guardará la BD
ENV DB_PATH=/app/data/library.db

# 4. Cambiamos al usuario sin privilegios por seguridad
USER app

ENTRYPOINT ["dotnet", "PdfLibraryApi.dll"]