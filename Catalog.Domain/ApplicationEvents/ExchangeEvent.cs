using Calabonga.OperationResults;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.ValueObjects;

namespace Catalog.Contracts.ApplicationEvents
{
    public sealed class ExchangeEvent : SimpleEntity
    {
        private readonly List<ExportedEntity> _entities = [];
        protected ExchangeEvent(TitleValue title, string code, Guid id, string? message) : base(title, code, id)
        {
            Message = message;
        }

        public DateTime ExecutedAt { get; private set; }
        public string Type { get; private set; }
        public string? Message { get; set; }

        public IReadOnlyCollection<ExportedEntity> Entities => _entities.AsReadOnly();

        public static Operation<ExchangeEvent, string> Create(string type, DateTime executedAt, string title = "default", string? message = null) {
            
            if (string.IsNullOrWhiteSpace(type))
                return Operation.Error("Type is empty or null");

            if (string.IsNullOrWhiteSpace(title))
                return Operation.Error("Title is empty or null");

            if (executedAt == default)
                return Operation.Error("Incorrect value ExecutedAt!");

            var titleValue = TitleValue.Create(title);

            if (!titleValue.Ok)
                return Operation.Error(titleValue.Error);

            var exchangeEvent = new ExchangeEvent(titleValue.Result, Guid.NewGuid().ToString(), Guid.Empty, message is null ? titleValue.Result.Value : message);

            exchangeEvent.ExecutedAt = executedAt;

            exchangeEvent.Type = type;

            exchangeEvent.Disable();

            return exchangeEvent;
        }

        public void Confirm() 
        { 
            Enabled  = true;
        }
    }
}
