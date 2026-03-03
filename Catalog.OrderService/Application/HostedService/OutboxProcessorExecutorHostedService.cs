using Calabonga.Microservices.BackgroundWorkers;
using Catalog.Contracts;

namespace Catalog.OrderService.Application.HostedService;
public sealed class OutboxProcessorExecutorHostedService : ScheduledHostedServiceBase
{
    public OutboxProcessorExecutorHostedService(IServiceScopeFactory serviceScopeFactory, ILogger<OutboxCleanerProcessorExecutorHostedService> logger)
        : base(serviceScopeFactory, logger)
    {
    }

    protected override async Task ProcessInScopeAsync(IServiceProvider serviceProvider, CancellationToken token)
    {
        var outboxProcessor = serviceProvider.GetRequiredService<IOutboxProcessor>();
        await outboxProcessor.ProcessAsync(serviceProvider, token);
    }

    protected override string Schedule => "0/30 * * * * *"; // every 30 seconds

    protected override bool IncludingSeconds => true;

    protected override string DisplayName => "OutboxProcessorExecutorHostedService";

    #if DEBUG
        protected override bool IsExecuteOnServerRestart => true;
    #else
        protected override bool IsExecuteOnServerRestart => false;
    #endif
}

public sealed class OutboxCleanerProcessorExecutorHostedService : ScheduledHostedServiceBase
{
    public OutboxCleanerProcessorExecutorHostedService(IServiceScopeFactory serviceScopeFactory, ILogger<OutboxCleanerProcessorExecutorHostedService> logger)
        : base(serviceScopeFactory, logger)
    {
    }

    protected override async Task ProcessInScopeAsync(IServiceProvider serviceProvider, CancellationToken token)
    {
        var outboxProcessor = serviceProvider.GetRequiredService<IOutboxCleanerProcessor>();
        await outboxProcessor.ProcessAsync(serviceProvider, token);
    }

    protected override string Schedule => "0 0 0 */10 * *"; // каждые 10 дней в полночь

    protected override bool IncludingSeconds => true;

    protected override string DisplayName => "OutboxCleanerProcessorExecutorHostedService";

#if DEBUG
    protected override bool IsExecuteOnServerRestart => true;
#else
        protected override bool IsExecuteOnServerRestart => false;
#endif
}