var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ITrade_ApiServices>("itrade-apiservices");

builder.Build().Run();
