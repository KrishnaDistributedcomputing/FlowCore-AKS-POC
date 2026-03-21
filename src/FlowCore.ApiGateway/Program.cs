var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/healthz");
app.MapReverseProxy();

app.Run();
