using Microsoft.EntityFrameworkCore;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Application.Interfaces;
using TradeLens.Infrastructure.Repositories;
using TradeLens.Domain.Services;
using TradeLens.Api.Services;
using TradeLens.Application.Transactions.Commands.AddTransaction;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TradeLensDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TradeLens")));

builder.Services.AddControllers();

builder.Services.AddScoped<AddTransactionService>();
builder.Services.AddScoped<PositionCalculator>();

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddScoped<IUnitOfWork, TradeLensUnitOfWork>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
