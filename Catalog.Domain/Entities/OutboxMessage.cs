using Catalog.Contracts.Interfaces;
using Catalog.Domain.Entities.Base;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Catalog.Contracts.Entities
{
    public sealed class OutboxMessage : Entity
    {
        public OutboxMessage(Guid id) : base(id)
        {
        }

        public static JsonSerializerOptions JsonSettings 
            => new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                };

        public required string Type { get; set; }

        public required string Content { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public string? Error { get; set; }
    }
}
