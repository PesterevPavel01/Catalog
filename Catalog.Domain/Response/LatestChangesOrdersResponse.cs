using System.Text.Json;
using Calabonga.OperationResults;
using Catalog.Contracts.Dto.Order;

namespace Catalog.Contracts.Response
{
    public sealed record LatestChangesOrdersResponse
    {
        public LatestChangesOrdersResponse(String latestChangesOrders)
        {
            LatestChangesOrders = latestChangesOrders;
        }

        public String LatestChangesOrders { get; private set; }
        public static Operation<LatestChangesOrdersResponse, String> Create(LatestChangesOrderDto model)
        {
            try
            {
                string jsonModel = JsonSerializer.Serialize(model,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                return new LatestChangesOrdersResponse(jsonModel);
            }
            catch (Exception ex) 
            {
                return Operation.Error(ex.Message);
            }
        }

        public Operation<LatestChangesOrderDto, String>  GetLatestChangesOrders() 
        {
            if (string.IsNullOrWhiteSpace(LatestChangesOrders))
                return Operation.Error("LatestChangesOrders is null or white space!");

            try
            {
                var model = JsonSerializer.Deserialize<LatestChangesOrderDto>(
                LatestChangesOrders,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }) ?? new LatestChangesOrderDto();

                return model;
            }
            catch (Exception ex)
            {
                return Operation.Error(ex.Message);
            }
        }
    };
}
