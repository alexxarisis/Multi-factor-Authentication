# <div  align="center">MULTIFACTOR AUTHENTICATION</div>

REST API with authorization and multi-factor authentication methods (currently only through email).
Provided dummy web client for ease of use.

## <div  align="center">REQUIREMENTS</div>

1. Visual Studio 2022 (preferred)
   i. ASP.NET and web development
   ii. .NET desktop development
2. .NET 6.0+
3. WSL2 (Windows)
4. Docker Desktop

## <div  align="center">HOW TO RUN</div>

### BACKEND
1. Run Docker Desktop.
2. Open the ```Backend.csproj``` file in Visual Studio.
3. Change the ```EmailService``` settings in ```appsettings.json```.
3. Run docker-compose.

### CLIENT
1. Open the ```Client.sln``` file in Visual Studio and run it.
2. A browser page will open up.