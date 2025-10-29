using Calabonga.OperationResults;
using Catalog.Domain.Entities.Base;

namespace Catalog.Domain.ValueObjects
{
    public class TextParameterValue : ValueObject
    {
        public const int MaxValueLength = 255;

        private TextParameterValue(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Operation<TextParameterValue, string> Create(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Operation.Error("Value is null or empty.");
            }

            if (code.Length > MaxValueLength)
            {
                return Operation.Error("Value length is greater than Max value.");
            }

            return new TextParameterValue(code);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}