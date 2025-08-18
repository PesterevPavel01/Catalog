using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calabonga.OperationResults;
using Catalog.Domain.Entities.Base;

namespace Catalog.Domain.ValueObjects
{
    public class PasswordValue : ValueObject
    {
        public const int MaxPasswordLength = 255;

        private PasswordValue(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static Operation<PasswordValue, string> Create(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return Operation.Error("Value is null or empty.");
            }

            if (password.Length > MaxPasswordLength)
            {
                return Operation.Error("Value length is greater than Max value.");
            }

            return new PasswordValue(password);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}