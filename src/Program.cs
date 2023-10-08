using Lighthouse.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

builder.Services.AddHttpClient();

builder.Services.RegisterApp(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseRouting();

app.AddDynamicWebHooks();

app.MapGet("/status", () => StatusCodes.Status200OK);

app.Run();

public partial class Program { }