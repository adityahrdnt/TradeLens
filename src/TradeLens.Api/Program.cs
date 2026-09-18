using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using System.Text.Json.Serialization;
using TradeLens.Api.Authentication;
using TradeLens.Api.Errors;
using TradeLens.Api.Services;
using TradeLens.Application.Interfaces;
using TradeLens.Application.Transactions.Commands.AddTransaction;
using TradeLens.Application.Transactions.Commands.CorrectTransaction;
using TradeLens.Application.Transactions.Queries.GetTransaction;
using TradeLens.Application.Validators;
using TradeLens.Application.Valuation.Queries.GetPositionValuation;
using TradeLens.Domain.Services;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Infrastructure.Persistence.Repositories;
using TradeLens.Infrastructure.Repositories;
using TradeLens.Infrastructure.Services;
using TradeLens.Application.Services;

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
builder.Services.AddScoped<CorrectTransactionService>();
builder.Services.AddScoped<GetTransactionService>();
builder.Services.AddScoped<PositionCalculator>();
builder.Services.AddScoped<ValuationCalculator>();
builder.Services.AddScoped<GetPositionValuationService>();

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
builder.Services.AddScoped<IPortfolioAccessService, PortfolioAccessService>();
builder.Services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
builder.Services.AddScoped<ITransactionRequestHasher, TransactionRequestHasher>();
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