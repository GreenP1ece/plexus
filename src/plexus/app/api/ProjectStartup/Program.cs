using Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPersistence(builder.Configuration)
    .AddTokenAuthentication(builder.Configuration, builder.Environment)
    .AddApiAuthorization()
    .AddApiCors()
    .AddApiControllers()
    .AddApiSwagger(builder.Configuration);

var app = builder.Build();

app.ApplyMigrations();

app
    .UseApiPipeline()
    .MapApiEndpoints();

app.Run();