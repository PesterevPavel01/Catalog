using Catalog.Domain.Entities.Base;
using Calabonga.OperationResults;

namespace Catalog.Domain.ValueObjects
{
    public class TitleValue : ValueObject
    {
        public const int MaxTitleLength = 256;

        private TitleValue(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Operation<TitleValue, string> Create(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Operation.Error("Value is null or empty.");
            }

            if (title.Length > MaxTitleLength)
            {
                return Operation.Error("Value length is greater than Max value.");
            }

            return new TitleValue(title);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}