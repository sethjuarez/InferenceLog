# Readme
Little app designed to create a table and insert _many_ records into it.

## SQL Server
Pull container
```
docker pull mcr.microsoft.com/mssql/server:2019-latest
```
Run container
```
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" `
   -p 1433:1433 --name sql1 --hostname sql1 `
   -d mcr.microsoft.com/mssql/server:2019-latest
```

## Project

Make sure to add the nuget source:
```
dotnet nuget add source --name nuget.org https://api.nuget.org/v3/index.json
```

