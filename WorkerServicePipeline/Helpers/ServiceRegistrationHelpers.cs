using Polly;
using Polly.Contrib.WaitAndRetry;
using System.Net;

namespace WorkerServicePipeline.Helpers
{
    public static class ServiceRegistrationHelpers
    {
        public static void RegisterAllSteps(IServiceCollection services)
        {
            var stepTypes = typeof(Abstractions.IStep).Assembly
                .GetTypes()
                .Where(t => typeof(Abstractions.IStep).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

            foreach (var type in stepTypes)
            {
                services.AddScoped(type);
            }
        }

        public static void RegisterHttpClients(IServiceCollection services)
        {
            var maxRetryAttempts = 3;
            var retryDelays = Backoff.ExponentialBackoff(
                TimeSpan.FromSeconds(1), maxRetryAttempts, fastFirst: true
            ).Select(delay => delay + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000))).ToArray();

            services.AddHttpClient<Apis.Interfaces.IFakeApiClient, Apis.Clients.FakeApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
            })
            .AddPolicyHandler((sp, req) => GetResiliencePolicy<Apis.Clients.FakeApiClient>(sp, retryDelays));
        }

        public static IAsyncPolicy<HttpResponseMessage> GetResiliencePolicy<T>(IServiceProvider sp, TimeSpan[] retryDelays)
        {
            var logger = sp.GetRequiredService<ILogger<T>>();
            var retry = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests || (int)r.StatusCode >= 500)
                .WaitAndRetryAsync(
                    retryDelays,
                    onRetry: (outcome, timespan, retryAttempt, ctx) =>
                    {
                        logger.LogWarning("Retry {RetryAttempt} after {Delay}ms due to {Reason}",
                            retryAttempt, timespan.TotalMilliseconds,
                            outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                    }
                );

            return retry;
        }
    }
}