using BiddingService.RequestHelpers;
using BiddingService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetEntryAssembly());

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("bids", false));

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.UseMessageRetry(r =>
        {
            r.Handle<RabbitMqConnectionException>();
            r.Interval(5, TimeSpan.FromSeconds(10));
        });

        cfg.Host(builder.Configuration["RabbitMq:Host"]);

        cfg.ConfigureEndpoints(ctx);
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["IdentityServiceUrl"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = "username";
    });

builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);

builder.Services.AddHostedService<CheckAuctionFinished>();

builder.Services.AddScoped<GrpcAuctionClient>();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

await Policy.Handle<TimeoutException>()
    .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
    .ExecuteAndCaptureAsync(async () =>
    {
        await DB.InitAsync("BidDb",
        MongoClientSettings.FromConnectionString(builder.Configuration.GetConnectionString("BidDbConnection")));
    });
    
app.Run();
