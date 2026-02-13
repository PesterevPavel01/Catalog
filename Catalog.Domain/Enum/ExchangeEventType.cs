namespace Catalog.Contracts.Enum
{
    public enum ExchangeEventType      
    {
        OrderSync = 0,
        ImportComponent=1,

    }
    public static class ExchangeEventTypeExtensions
    {
        public static string ToRussianString(this ExchangeEventType type)
        {
            return type switch
            {
                ExchangeEventType.OrderSync => "Синхронизация заказов с 1с",
                ExchangeEventType.ImportComponent => "Импорт компонентов из 1с",
                _ => type.ToString()
            };
        }


    }
}
