FROM bitnami/dotnet-sdk:7 AS build-env
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM bitnami/aspnet-core:7
EXPOSE 80
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Library.dll", "--urls", "http://0.0.0.0:80"]
HEALTHCHECK --start-period=30s --interval=5s CMD curl --silent --fail --insecure https://localhost:80/ || exit 1
