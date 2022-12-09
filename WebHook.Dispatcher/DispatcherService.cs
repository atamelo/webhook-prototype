using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class DispatcherService : IHostedService
{
    private readonly DispatcherLoop dispatcherLoop;
    private readonly ILogger<DispatcherService> logger;
    private readonly CancellationTokenSource cancellation;

    private Task dispatcherLoopTask;
    public DispatcherService(
        DispatcherLoop dispatcherLoop,
        ILogger<DispatcherService> logger)
    {
        this.dispatcherLoop = dispatcherLoop;
        this.logger = logger;
        this.cancellation = new CancellationTokenSource();
        this.dispatcherLoopTask = Task.CompletedTask;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Starting the service...");

        this.dispatcherLoopTask = Task.Factory.StartNew(() =>
        {
            // NOTE: when directly implementing IHostedService exceptions like this won't surface
            // until the stop (Ctrl-C) the service
            // Consider inheriting from BackgroundService instead
            //throw null;
            this.dispatcherLoop.Start(this.cancellation.Token);
        }, TaskCreationOptions.LongRunning);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Stopping the service...");

        this.cancellation.Cancel();
        // NOTE: here is the point where exceptions are going to surface
        await dispatcherLoopTask;

        this.logger.LogInformation("Service stopped.");
    }
}
