using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using FluentValidation;
using TestTask.Application.Interfaces;
using TestTask.Application.Services;
using TestTask.Application.Strategies;
using TestTask.Infrastructure;
using TestTask.Infrastructure.Data;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());


builder.Services.AddDbInfrastructure(builder.Configuration);

builder.Services.AddScoped<ITransactionStrategy, CreditStrategy>();
builder.Services.AddScoped<ITransactionStrategy, DebitStrategy>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
    DataSeeder.SeedDatabase(context);
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


app.Run();