using Calabonga.OperationResults;
using Catalog.Domain.Entities.Base;

namespace Catalog.Domain.ValueObjects
{
    public class CodeValue : ValueObject
    {
        public const int MaxCodeLength = 36;

        private CodeValue(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Operation<CodeValue, string> Create(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Operation.Error("Value is null or empty.");
            }

            if (code.Length > MaxCodeLength)
            {
                return Operation.Error("Value length is greater than Max value.");
            }

            return new CodeValue(code);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}