using Lighthouse.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Services.AddHttpClient();

builder.Configuration.AddEnvironmentVariables();

builder.RegisterApp(builder.Configuration);

var app = builder.Build();

app.MapHealthChecks("/health");

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseRouting();

app.AddDynamicWebHooks();

app.MapGet("/status", () => StatusCodes.Status200OK);

app.Run();

public partial class Program { }