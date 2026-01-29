
using Catalog.Contracts.Interfaces;

namespace Catalog.Domain.Entities.Base
{
    public class Auditable : IAuditable
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
