var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddPostgres("db")
    .WithDataVolume()
    .WithPgWeb()
    .AddDatabase("ITradeDB");

builder.AddProject<Projects.ITrade_ApiServices>("itrade-apiservices")
    .WithReference(db)
    .WaitFor(db);

builder.Build().Run();
