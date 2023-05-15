# aspnetlibraryschool

To use this application locally in development mode, first start a MySql database using docker-compose "docker-compose up database", and start the app using "dotnet run" or VS(not tested).

To run this on docker completely in production, copy paste:

"ConnectionStrings": {
        "DefaultConnection": "Server=database;Port=3306;Database=library;Uid=app;Pwd=testpassword;AllowLoadLocalInfile=true"
    },
    
and put this in second line of appsettings.json, just after the first curly brace {.
Then build the docker using "docker-compose build".

If you want to run this without docker, you're going to have to switch to your own database (providing connection strings and setting correct database types in Program.cs). If there's an issue setting initial migration, try deleting all migrations inside Migrations folder.
