using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities.Base
{
    public class SimpleEntity : Entity
    {
        protected SimpleEntity(TitleValue title, string code, Guid id) : base(id)
        {
            Title = title;
            Code = code;
        }

        public TitleValue Title { get; protected set; }

        public string Code { get; protected set; }
    }
}
