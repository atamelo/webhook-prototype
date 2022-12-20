using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class DispatcherService : IHostedService
{
    private readonly CancellationTokenSource cancellation;
    private readonly DispatcherLoop dispatcherLoop;
    private readonly ILogger<DispatcherService> logger;
    private Task dispatcherLoopTask;

    public DispatcherService(
        DispatcherLoop dispatcherLoop,
        ILogger<DispatcherService> logger)
    {
        this.dispatcherLoop = dispatcherLoop;
        this.logger = logger;
        cancellation = new CancellationTokenSource();
        dispatcherLoopTask = Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting the service...");

        dispatcherLoopTask = Task.Factory.StartNew(() =>
        {
            // NOTE: when directly implementing IHostedService exceptions like this won't surface
            // until the stop (Ctrl-C) the service
            // Consider inheriting from BackgroundService instead
            //throw null;
            dispatcherLoop.Start(cancellation.Token);
        }, TaskCreationOptions.LongRunning);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping the service...");

        cancellation.Cancel();
        // NOTE: here is the point where exceptions are going to surface
        await dispatcherLoopTask;

        logger.LogInformation("Service stopped.");
    }
}