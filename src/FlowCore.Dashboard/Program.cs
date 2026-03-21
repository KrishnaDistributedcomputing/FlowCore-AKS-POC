using FlowCore.Dashboard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHealthChecks();

builder.Services.AddHttpClient<FlowCoreApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiGateway:BaseUrl"] ?? "http://api-gateway.platform.svc.cluster.local");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient<AzureResourceClient>(client =>
{
    client.BaseAddress = new Uri("https://management.azure.com");
    client.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddSingleton<AzureResourceClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapHealthChecks("/healthz");
app.MapFallbackToPage("/_Host");

app.Run();
