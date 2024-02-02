using Polly.Extensions.Http;
using Polly;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHttpClient("AuctionSvc", client => client.BaseAddress =
    new Uri(config.GetValue<string>("AuctionServiceUrl") + "/api"))
    .AddPolicyHandler(GetRetryPolicy());

builder.Services.AddScoped<AuctionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

try
{
	await DbInitializer.InitDb(app);
}
catch (Exception ex)
{
	Console.WriteLine(ex.Message);
}

app.Run();

IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    // Retry with jitter: https://github.com/App-vNext/Polly/wiki/Retry-with-jitter
    Random jitter = new();

    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 5,
            sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))  // exponential backoff (2, 4, 8, 16, 32 secs)
                  + TimeSpan.FromMilliseconds(jitter.Next(0, 1000))  // plus some jitter: up to 1 second
            );
}