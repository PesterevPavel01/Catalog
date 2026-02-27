namespace Catalog.Contracts
{
    public interface IOutboxProcessor
    {
        Task ProcessAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }
}
