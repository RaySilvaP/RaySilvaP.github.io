FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /source

COPY ./*.csproj ./Backend/
COPY ./Shared ./Shared
RUN dotnet restore ./Backend/

COPY . ./Backend
RUN rm -r ./Backend/Shared
RUN dotnet publish ./Backend -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0

EXPOSE 8080
WORKDIR /app

COPY --from=build /app .
ENTRYPOINT [ "./Backend" ]