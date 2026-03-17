using Microsoft.Extensions.DependencyInjection;
using TestTask.Infrasturcture.DI;

namespace TestTask.App;

internal class Program
{
    static async Task Main(string[] args)
    {
        var serviceCollection = new ServiceCollection()
            .AddSingleton<Application>()
            .AddSingleton<XbrlProcessor>()
            .RegisterInfrastructure();

        using (var serviceProviser = serviceCollection.BuildServiceProvider())
        {
            using (var cts = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };

                try
                {
                    await serviceProviser.GetRequiredService<Application>().RunAsync(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation was canceled.");
                }
            }
        }
    }
}
