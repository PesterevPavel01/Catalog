using Calabonga.OperationResults;

namespace Catalog.Contracts.ApplicationEvents
{
    public class ExportedEntity
    {
        protected ExportedEntity(Guid id, string entityType, string code)
        {
            Id = id;
            EntityType = entityType;
            Code = code;

        }

        public ExchangeEvent ExchangeEvent { get; private set; } = null!;
        public Guid ExchangeEventId { get; private set; }
        public Guid Id { get; private set; }
        public String Code { get; private set; }
        public string EntityType { get; private set; } = null!;

        public static ExportedEntity Create(ExchangeEvent exchangeEvent, string entityType, string code) 
            => 
            new ExportedEntity(Guid.Empty, entityType, code)
            .SetExchangeEvent(exchangeEvent);

        private ExportedEntity SetExchangeEvent(ExchangeEvent exchangeEvent) 
        {
            ExchangeEvent = exchangeEvent;
            return this;
        }
    }
}
