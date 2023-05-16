# aspnetlibraryschool

To use this application locally in development mode, first start a MySql database using docker-compose "docker-compose up database", and start the app using "dotnet run" or VS(not tested).

To run this on docker in production, copy paste:

"ConnectionStrings": {
        "DefaultConnection": "Server=database;Port=3306;Database=library;Uid=app;Pwd=testpassword;AllowLoadLocalInfile=true"
    },
    
and put this in second line of appsettings.json, just after the first curly brace {.
Then build the docker using "docker-compose build" and run it using "docker-compose up".

If you don't have access to docker, you're forced to change a couple of things to run it on SQLite.

1. Replace the contents of "DefaultConnection" string to "Data Source=app.db" (The entire key:value pair should look like "DefaultConnection": "Data Source=app.db"
2. Inside Program.cs, change the line 
options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
with
options.UseSqlite(connectionString);
3. Delete everything inside Migrations folder at the root of the project
4. Generate new migrations using dotnet ef migrations add IntialSetup
5. Update the database using dotnet ef database update
After that, you're ready to run the application locally.
