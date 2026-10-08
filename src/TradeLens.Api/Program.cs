using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using TradeLens.Api.Authentication;
using TradeLens.Api.BackgroundServices;
using TradeLens.Api.Errors;
using TradeLens.Api.Middleware;
using TradeLens.Api.Services;
using TradeLens.Application.CorporateActions.Commands.ApplyCorporateAction;
using TradeLens.Application.CorporateActions.Commands.CancelCorporateAction;
using TradeLens.Application.CorporateActions.Commands.CreateCorporateAction;
using TradeLens.Application.CorporateActions.Commands.CorrectCorporateAction;
using TradeLens.Application.CorporateActions.Commands.DelayCorporateAction;
using TradeLens.Application.CorporateActions.Queries.GetCorporateAction;
using TradeLens.Application.CorporateActions.Queries.GetCorporateActionsByInstrument;
using TradeLens.Application.Interfaces;
using TradeLens.Application.MarketPrices;
using TradeLens.Application.Pnl.Queries.GetPortfolioPnl;
using TradeLens.Application.Portfolios.Queries.GetPortfolios;
using TradeLens.Application.Positions.Queries.GetPortfolioPositions;
using TradeLens.Application.Services;
using TradeLens.Application.Transactions.Commands.AddTransaction;
using TradeLens.Application.Transactions.Commands.CorrectTransaction;
using TradeLens.Application.Transactions.Commands.VoidTransaction;
using TradeLens.Application.Transactions.Queries.GetPortfolioTransactions;
using TradeLens.Application.Transactions.Queries.GetTransaction;
using TradeLens.Application.Validators;
using TradeLens.Application.Valuation.Queries.GetPortfolioValuation;
using TradeLens.Application.Valuation.Queries.GetPositionValuation;
using TradeLens.Domain.Services;
using TradeLens.Infrastructure.MarketPrices.YahooFinance;
using TradeLens.Infrastructure.Persistence;
using TradeLens.Infrastructure.Persistence.Repositories;
using TradeLens.Infrastructure.Repositories;
using TradeLens.Infrastructure.Services;
using TradeLens.Api.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<MarketPriceOptions>()
    .Bind(builder.Configuration.GetSection(
        MarketPriceOptions.SectionName))
    .Validate(
        options => options.SyncIntervalMinutes > 0,
        "Market price sync interval must be greater than zero.")
    .Validate(
        options => options.MaxAgeMinutes > 0,
        "Market price maximum age must be greater than zero.")
    .ValidateOnStart();

builder.Services
    .AddOptions<YahooFinanceOptions>()
    .Bind(builder.Configuration.GetSection(
        YahooFinanceOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.BaseUrl),
        "Yahoo Finance base URL is required.")
    .ValidateOnStart();

builder.Services
    .AddHttpClient(
        "YahooFinance",
        (serviceProvider, client) =>
        {
            var options =
                serviceProvider
                    .GetRequiredService<
                        IOptions<YahooFinanceOptions>>()
                    .Value;

            client.BaseAddress =
                new Uri(options.BaseUrl);

            client.Timeout =
                TimeSpan.FromSeconds(10);
        })
    .AddStandardResilienceHandler();

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

builder.Services.AddScoped<MarketPriceSyncService>();

builder.Services.AddScoped<IMarketPriceFreshnessPolicy>(
    serviceProvider =>
    {
        var options =
            serviceProvider
                .GetRequiredService<
                    IOptions<MarketPriceOptions>>()
                .Value;

        return new MarketPriceFreshnessPolicy(
            TimeSpan.FromMinutes(
                options.MaxAgeMinutes));
    });

builder.Services.AddScoped<IMarketPriceSyncJob, MarketPriceSyncJob>();

builder.Services.AddScoped<
    IMarketPriceProvider,
    YahooFinanceMarketPriceProvider>();

builder.Services.AddScoped<AddTransactionService>();
builder.Services.AddScoped<CorrectTransactionService>();
builder.Services.AddScoped<VoidTransactionService>();
builder.Services.AddScoped<GetTransactionService>();
builder.Services.AddScoped<GetPortfolioTransactionsService>();
builder.Services.AddScoped<GetPortfolioPositionsService>();
builder.Services.AddScoped<GetPortfolioPnlService>();
builder.Services.AddScoped<GetPortfoliosService>();
builder.Services.AddScoped<CreateCorporateActionService>();
builder.Services.AddScoped<GetCorporateActionService>();
builder.Services.AddScoped<ApplyCorporateActionService>();
builder.Services.AddScoped<CancelCorporateActionService>();
builder.Services.AddScoped<GetCorporateActionsByInstrumentService>();
builder.Services.AddScoped<DelayCorporateActionService>();
builder.Services.AddScoped<CorrectCorporateActionService>();
builder.Services.AddScoped<PositionCalculator>();
builder.Services.AddScoped<CorporateActionCalculator>();
builder.Services.AddScoped<CorporateActionPositionCalculator>();
builder.Services.AddScoped<ValuationCalculator>();
builder.Services.AddScoped<GetPositionValuationService>();
builder.Services.AddScoped<GetPortfolioValuationService>();

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ICorporateActionRepository, CorporateActionRepository>();
builder.Services.AddScoped<ICorporateActionApplicationRepository, CorporateActionApplicationRepository>();
builder.Services.AddScoped<ICorporateActionChangeRepository, CorporateActionChangeRepository>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
builder.Services.AddScoped<IInstrumentRepository, InstrumentRepository>();
builder.Services.AddScoped<IMarketPriceRepository, MarketPriceRepository>();
builder.Services.AddScoped<IPortfolioAccessService, PortfolioAccessService>();
builder.Services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
builder.Services.AddScoped<ITransactionRequestHasher, TransactionRequestHasher>();
builder.Services.AddScoped<IUnitOfWork, TradeLensUnitOfWork>();

builder.Services.AddScoped<GetPortfolioPnlQueryValidator>();
builder.Services.AddScoped<CreateCorporateActionCommandValidator>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<JwtTokenGenerator>();

builder.Services.AddHostedService<
    MarketPriceSyncBackgroundService>();

builder.Services.AddAuthorization();

if (builder.Environment.IsDevelopment())
{
    builder.Services
        .AddAuthentication("Development")
        .AddScheme<
            AuthenticationSchemeOptions,
            DevelopmentAuthenticationHandler>(
            "Development",
            _ => { });
}
else
{
    builder.Services
        .AddAuthentication("Bearer")
        .AddJwtBearer("Bearer");

    builder.Services
        .AddOptions<JwtBearerOptions>("Bearer")
        .Configure<IOptions<JwtOptions>>(
            (options, jwtOptions) =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer =
                            jwtOptions.Value.Issuer,

                        ValidateAudience = true,
                        ValidAudience =
                            jwtOptions.Value.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(
                                    jwtOptions.Value.SecretKey)),

                        ValidateLifetime = true,

                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
            });
}

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<TradeLensExceptionHandler>();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();

public partial class Program
{
}
