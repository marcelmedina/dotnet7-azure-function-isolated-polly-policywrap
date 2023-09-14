using consumer.TypedHttpClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using System.Net;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        var currentDirectory = hostingContext.HostingEnvironment.ContentRootPath;

        config.SetBasePath(currentDirectory)
            .AddJsonFile("settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
        config.Build();
    })
    .ConfigureServices((services) =>
    {
        var fallbackPolicy = Policy
            .HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
            .FallbackAsync(_ =>
            {
                Console.Out.WriteLine("### Fallback executed");
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) // handle the way you want, this is a demo
                {
                    Content = new StringContent("-1")
                });
            });

        var retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
            .WaitAndRetryAsync(3,
                _ => TimeSpan.FromMilliseconds(1000),
                onRetry: (message, timeSpan) =>
                {
                    Console.Out.WriteLine("----------------------------------------------------");
                    Console.Out.WriteLine($"### RequestMessage: {message.Result.RequestMessage}");
                    Console.Out.WriteLine($"### StatusCode: {message.Result.StatusCode}");
                    Console.Out.WriteLine($"### ReasonPhrase: {message.Result.ReasonPhrase}");
                    Console.Out.WriteLine($"### TimeSpan: {timeSpan}");
                    Console.Out.WriteLine("----------------------------------------------------");
                });

        var circuitBreakPolicy = Policy
            .HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
            .CircuitBreakerAsync(3, 
                TimeSpan.FromSeconds(10),
                onBreak: (_, _) =>
                {
                    Console.Out.WriteLine("*****Open*****");
                },
                onReset: () =>
                {
                    Console.Out.WriteLine("*****Closed*****");
                },
                onHalfOpen: () =>
                {
                    Console.Out.WriteLine("*****Half Open*****");
                });

        //var policyWrap = Policy.WrapAsync<HttpResponseMessage>(fallbackPolicy, retryPolicy, circuitBreakPolicy);
        var policyWrap = Policy.WrapAsync<HttpResponseMessage>(fallbackPolicy, retryPolicy);

        services.AddHttpClient<StateCounterHttpClient>()
            .AddPolicyHandler(policyWrap);
    })
    .Build();

host.Run();
