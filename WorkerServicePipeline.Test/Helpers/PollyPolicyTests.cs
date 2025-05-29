using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using RichardSzalay.MockHttp;
using System.Net;

public class PollyPolicyTests
{
    [Fact]
    public async Task HttpClient_ShouldRetry_OnTransientError()
    {
        // Arrange  
        var retryCount = 0;
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*").Respond(_ =>
        {
            retryCount++;
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        });

        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole(); // This requires Microsoft.Extensions.Logging.Console namespace  
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        services.AddHttpClient("test")
            .ConfigurePrimaryHttpMessageHandler(() => mockHttp)
            .AddPolicyHandler((sp, req) =>
            {
                var logger = sp.GetRequiredService<ILogger<PollyPolicyTests>>();
                var retry = Policy<HttpResponseMessage>
                    .HandleResult(r => (int)r.StatusCode >= 500)
                    .WaitAndRetryAsync(
                        2,
                        _ => TimeSpan.Zero,
                        (outcome, timespan, attempt, ctx) => logger.LogWarning("Retry {Attempt}", attempt)
                    );
                return retry;
            });

        var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("test");

        // Act  
        var response = await client.GetAsync("http://test/retry");

        // Assert  
        Assert.Equal(3, retryCount); // 1 initial + 2 retries  
    }
}