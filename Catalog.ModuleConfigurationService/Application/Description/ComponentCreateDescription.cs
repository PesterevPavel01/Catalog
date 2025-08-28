namespace Catalog.ModuleConfigurationService.Application.Description
{
    public static class ComponentCreateDescription
    {
        public static string GetValue()
        {
            var description = @"{
                ""moduleType"": ""Фасад"",
                ""moduleTypeCode"": ""00000000FSD"",
                ""numericParameters"": [
                {
                    ""type"": ""Ширина"",
                    ""typeCode"": ""0000000WDHT"",
                    ""value"": 200
                },
                {
                    ""type"": ""Длина"",
                    ""typeCode"": ""000000LNGHT"",
                    ""value"": 400
                }
                ],
                ""textParameters"": [
                {
                    ""type"": ""Тон"",
                    ""typeCode"": ""000000000TN"",
                    ""value"": ""Матовый""
                }
                ]
            }";

            return description;
        }
    }
}
