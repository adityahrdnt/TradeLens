using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Application.Interfaces;
using TradeLens.Infrastructure.Repositories;
using TradeLens.Domain.Services;
using TradeLens.Api.Services;
using FluentValidation;
using TradeLens.Application.Transactions.Commands.AddTransaction;
using TradeLens.Api.Authentication;
using System.Text.Json.Serialization;
using TradeLens.Api.Errors;
using TradeLens.Application.Validators;
using TradeLens.Application.Transactions.Queries.GetTransaction;
using TradeLens.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TradeLensDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TradeLens")));

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddValidatorsFromAssemblyContaining<
    AddTransactionCommandValidator>();

builder.Services.AddScoped<AddTransactionService>();
builder.Services.AddScoped<GetTransactionService>();
builder.Services.AddScoped<PositionCalculator>();

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
builder.Services.AddScoped<IUnitOfWork, TradeLensUnitOfWork>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services
    .AddAuthentication("Development")
    .AddScheme<AuthenticationSchemeOptions, DevelopmentAuthenticationHandler>(
        "Development",
        _ => { });

builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<TradeLensExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program
{
}