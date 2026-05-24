Top run the project, you will need to edit the appsettings.json file and add your own connection string to the database. I used SQL Server.
Then run the following command in the terminal to apply the migrations and create the database:
In the Package Manager Console: Update-Database

I write three unit tests for the project, but I would have liked to have written more if I had more time. The tests are located in the StadiumGateSystem.Tests project.
These tests must be run individually as they re-use the in-memory database and will interfere with each other if run sequentially.

When the site is running I used Swagger to test the API endpoints. 


If I had more time I would have liked to have added a few more features to this project.
Unit of Work
JWT
Identity authentication
DTOs
AutoMapper