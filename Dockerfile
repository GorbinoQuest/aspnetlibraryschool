FROM bitnami/dotnet-sdk:latest AS build-env
WORKDIR /App
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM bitnami/aspnet-core:latest
WORKDIR /App
COPY --from=build-env /App/out .
EXPOSE 5000
ENTRYPOINT ["dotnet", "Library.dll"]
