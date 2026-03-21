using FlowCore.Dashboard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHealthChecks();

builder.Services.AddHttpClient<FlowCoreApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiGateway:BaseUrl"] ?? "http://api-gateway.platform.svc.cluster.local");
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.MapHealthChecks("/healthz");

app.Run();
