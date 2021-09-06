# AintBnB
.Net Core (C#) UWP and Blazor WebAssembly airbnb like application for renting and renting out holiday accommodations. If you want to run UWP then the startup projects must be both AintBnB.BlazorWASM.Server and AintBnB.App; if you only want to run Blazor then the only startup project must be AintBnB.BlazorWASM.Server.

Uses EF core for persistance. Add a database by entering the connection string in appsettings.json (file must be created inside AintBnB.BlazorWASM.Server).
the appsettings.json should be like:
"ConnectionStrings": {
    "DefaultConnection": //connection string goes here
  }

Unit and integration tests use temporary in memory databases and don't need to be setup with a real database to be executed!
