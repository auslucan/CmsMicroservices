using Polly;
using Polly.Extensions.Http;
using Serilog;

namespace ContentService.Helpers
{
    public static class PolicyHelper
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (outcome, timespan, retryAttempt, context) =>
                    {
                        Log.Warning("Retry {RetryAttempt} after {Delay}s due to {Reason}",
                            retryAttempt, timespan.TotalSeconds, outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString());
                    });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    2,
                    TimeSpan.FromSeconds(30),
                    (outcome, timespan) =>
                    {
                        Log.Warning("Circuit breaker opened for {Duration}s due to {Reason}",
                            timespan.TotalSeconds, outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString());
                    },
                    () =>
                    {
                        Log.Information("Circuit breaker reset.");
                    });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(
                10,
                onTimeoutAsync: (context, timespan, task, exception) =>
                {
                    Log.Warning("Timeout after {Timeout}s.", timespan.TotalSeconds);
                    return Task.CompletedTask;
                });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
        {
            return Policy.WrapAsync(
                GetTimeoutPolicy(),
                GetRetryPolicy(),
                GetCircuitBreakerPolicy()
            );
        }
    }
}
