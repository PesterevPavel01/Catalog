namespace Catalog.Contracts.Request
{
    public sealed record LatestChangesOrdersRequest(DateTime lastExchangeDate, DateTime currentExchangeDate);
}
