using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Domain.Entities;

namespace Catalog.FacadeOrderTitleValidator
{
    public class TitleValidator : ITitleValidator
    {
        private readonly IUnitOfWork _unitOfWork;

        public TitleValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<string, string>> Validate(CancellationToken cancellationToken)
        {
            var lastOrder = await _unitOfWork.GetRepository<Order>().GetFirstOrDefaultAsync(orderBy: q => q.OrderByDescending(o => o.CreatedAt));

            if (lastOrder is null)
            {
                return "1";
            }

            var lastValue = lastOrder.Title.Value;

            if (Microsoft.VisualBasic.Information.IsNumeric(lastValue))
                return (Convert.ToInt32(lastValue) + 1).ToString();
            else
                return Operation.Error("Last title is not numeric!");
        }
    }
}
