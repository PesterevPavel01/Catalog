namespace Catalog.ExchangeService.Application.Description
{
    internal static class ComponentCreateEndpointDescription
    {
        public static string Description =>
            @"Пример запроса:<br>
                {
                  ""componentCode"": ""00080184744"",
                  ""componentTitle"": ""Фрезеровка№1"",
                  ""componentTypeCode"": ""0000000FRZK"",
                  ""componentTypeTitle"": ""Фрезеровка"",
                  ""numericParameters"": [
                    {
                      ""type"": ""Минимальная ширина"",
                      ""typeCode"": ""00000MWDHT"",
                      ""value"": 150
                    },
                    {
                      ""type"": ""Тип фрезеровки"",
                      ""typeCode"": ""00000TPFRZ"",
                      ""value"": 1
                    }
                  ]
                }<br><br>
                {
                  ""componentCode"": ""00080196471"",
                  ""componentTitle"": ""Пленка матовая Венге рифленый 30209-22"",
                  ""componentTypeCode"": ""00080195637"",
                  ""componentTypeTitle"": ""ПЛЕНКА ПВХ"",
                  ""textParameters"": [
                    {
                      ""type"": ""Тон"",
                      ""typeCode"": ""00000000TN"",
                      ""value"": ""Глянец""
                    }
                  ],
                  ""numericParameters"": [
                    {
                      ""type"": ""Тип фрезеровки"",
                      ""typeCode"": ""00000TPFRZ"",
                      ""value"": 1
                    },
                    {
                        ""type"": ""Минимальная ширина"", 
                        ""typeCode"": ""00000MWDHT"", 
                        ""value"": 200 
                    }
                  ]
                }<br><br>
                {
                  ""componentCode"": ""00080185745"",
                  ""componentTitle"": ""Фрезеровка №1"",
                  ""componentTypeCode"": ""0000000FRZK"",
                  ""componentTypeTitle"": ""Фрезеровка"",
                  ""textParameters"": [
                    {
                      ""type"": ""Тон"",
                      ""typeCode"": ""00000000TN"",
                      ""value"": ""Матовый""
                    }
                  ],
                  ""numericParameters"": [
                    {
                      ""type"": ""Тип фрезеровки"",
                      ""typeCode"": ""00000TPFRZ"",
                      ""value"": 1
                    }
                  ]
                }<br><br>
                {
                  ""componentCode"": ""00080185745"",
                  ""componentTitle"": ""Нестандартный"",
                  ""componentTypeCode"": ""0000000FRZK"",
                  ""componentTypeTitle"": ""Фрезеровка"",
                  ""textParameters"": [
                    {
                      ""type"": ""Тон"",
                      ""typeCode"": ""00000000TN"",
                      ""value"": ""Матовый""
                    },
                    {
                      ""type"": ""Чертеж"",
                      ""typeCode"": ""0000000IMG"",
                      ""value"": ""IMG80185745.jpeg""
                    }
                  ],
                  ""numericParameters"": [
                    {
                      ""type"": ""Тип фрезеровки"",
                      ""typeCode"": ""00000TPFRZ"",
                      ""value"": 1
                    }
                  ]
                }<br><br>
{
                  ""componentCode"": ""00080185745"",
                  ""componentTitle"": ""Нестандартный"",
                  ""componentTypeCode"": ""0000000FRZK"",
                  ""componentTypeTitle"": ""Фрезеровка"",
                  ""textParameters"": [
                    {
                      ""type"": ""Тон"",
                      ""typeCode"": ""00000000TN"",
                      ""value"": ""Матовый""
                    },
                    {
                      ""type"": ""Чертеж"",
                      ""typeCode"": ""0000000IMG"",
                      ""value"": ""IMG80185745.jpeg""
                    }
                  ],
                  ""numericParameters"": [
                    {
                      ""type"": ""Тип фрезеровки"",
                      ""typeCode"": ""00000TPFRZ"",
                      ""value"": 1
                    },
                    { 
                        ""type"": ""Минимальная ширина"", 
                        ""typeCode"": ""00000MWDHT"", 
                        ""value"": 200 
                    }      
                  ]
                }";

    }
}
