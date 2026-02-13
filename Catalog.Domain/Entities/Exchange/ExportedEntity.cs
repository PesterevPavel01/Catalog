using Calabonga.OperationResults;

namespace Catalog.Contracts.Entities.Exchange
{
    public class ExportedEntity
    {
        public static short ErrorMaxLength = 500;
        protected ExportedEntity(Guid id, string entityType, string code)
        {
            Id = id;
            EntityType = entityType;
            Code = code;
        }

        public ExchangeEvent ExchangeEvent { get; private set; } = null!;
        public Guid ExchangeEventId { get; private set; }
        public Guid Id { get; private set; }
        public string Code { get; private set; }
        public string? Error { get; private set; }
        public string EntityType { get; private set; } = null!;
        
        public bool Ok() => Error is null; 

        public static ExportedEntity Create(ExchangeEvent exchangeEvent, string entityType, string code) 
            => 
            new ExportedEntity(Guid.Empty, entityType, code)
            .SetExchangeEvent(exchangeEvent);

        private ExportedEntity SetExchangeEvent(ExchangeEvent exchangeEvent) 
        {
            ExchangeEvent = exchangeEvent;
            return this;
        }

        public Operation<ExportedEntity, string> SetErrorMessage(string message) 
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Operation.Error("Message is null or empty.");
            }

            Error = message.Length > ErrorMaxLength ? message[..ErrorMaxLength] : message;

            return this;
        }
    }
}
