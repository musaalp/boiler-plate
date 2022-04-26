using Api.Extensions;
using Data.Extensions;
using Sdk.Api.Middlewares;
using Sdk.Extensions;
using Service.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add layers
builder.Services.AddSdk(builder.Configuration);
builder.Services.AddApi(builder.Configuration);
builder.Services.AddService(builder.Configuration);
builder.Services.AddData(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCustomExceptionHandler();
app.UseAuthorization();
app.MapControllers();
app.UseAuthorization();
app.Run();
